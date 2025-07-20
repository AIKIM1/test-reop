/*************************************************************************************
 Created Date : 2022.03.22
         Creator : 윤기업
      Decription : Coater Manual Input
--------------------------------------------------------------------------------------
 [Change History]
  2022.03.22 윤기업 : 최초생성
  2023.12.27 조성근 믹서코터 배치 연계 업그레이드
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.Popup
{

    public partial class CMM_ELEC_COATER_MANUAL_INPUT : C1Window, IWorkArea
    {
        #region Declare Variable
        private string _AREAID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTPSTNID = string.Empty;

        private string _MTRLID = string.Empty;


        bool bFinishLoading = false;


        CommonCombo _combo = new CommonCombo();

        public CMM_ELEC_COATER_MANUAL_INPUT()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Init Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps == null)
                    return;

                bFinishLoading = false;

                _AREAID = Util.NVC(tmps[0]);
                _EQPTID = Util.NVC(tmps[1]);
                _EQPTPSTNID = Util.NVC(tmps[1]);

                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sCase: "AREA");

                if (_AREAID != "") cboArea.SelectedValue = _AREAID;



                SetEquipmentMultiCombo(cboArea, cboEqptSearch);
                SetCommonCodeMultiCombo("ROLLMAP_SLURRY_MOUNT_PSTN_ID", cboEqptMeasrPstnSearch);


                SetEquipmentCombo();
                SetEqptPstnComboBox();

                dtFrom.SelectedDateTime = DateTime.Now.AddDays(-7);

                GetBatchList();

                bFinishLoading = true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Control Event
        private void dgBatchListChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    dgBatchList.SelectedIndex = idx;

                    ClearDlg();

                    double OUTPUT_QTY = 0;
                    double SUM_OUTPUT_QTY = 0;

                    INPUT_LOTID.Text = DataTableConverter.GetValue(dgBatchList.Rows[idx].DataItem, "INPUT_LOTID").ToString();
                    cboEqpt.SelectedValue = DataTableConverter.GetValue(dgBatchList.Rows[idx].DataItem, "EQPTID").ToString();
                    cboEqptPstnID.SelectedValue = DataTableConverter.GetValue(dgBatchList.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID").ToString();

                    OUTPUT_QTY = Convert.ToDouble(DataTableConverter.GetValue(dgBatchList.Rows[idx].DataItem, "OUTPUT_QTY"));
                    SUM_OUTPUT_QTY = Convert.ToDouble(DataTableConverter.GetValue(dgBatchList.Rows[idx].DataItem, "SUM_OUTPUT_QTY"));

                    _MTRLID = DataTableConverter.GetValue(dgBatchList.Rows[idx].DataItem, "MTRLID").ToString();
                    ACT_QTY.Value = 0;

                    if (OUTPUT_QTY > SUM_OUTPUT_QTY) ACT_QTY.Value = OUTPUT_QTY - SUM_OUTPUT_QTY;


                    

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bFinishLoading == false) return;
            if (cboArea.SelectedIndex < 0) return;

            SetEquipmentMultiCombo(cboArea, cboEqptSearch);
            SetEquipmentCombo();
        }

        private void cboEqpt_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bFinishLoading == false) return;
            if (cboEqpt.SelectedIndex < 0) return;

            SetEqptPstnComboBox();
        }

        private void cboEqptSearch_SelectionChanged(object sender, EventArgs e)
        {
            if (bFinishLoading == false) return;
            if (cboEqptSearch.SelectedItems.Count < 0) return;

            SetCommonCodeMultiCombo("ROLLMAP_SLURRY_MOUNT_PSTN_ID", cboEqptMeasrPstnSearch);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetBatchList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (checkValues() == false) return;

            Util.MessageConfirm("SFU1130", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveData();
                }
            });
        }

        private bool checkValues()
        {
            if (INPUT_LOTID.Text == "")
            {
                Util.MessageValidation("FM_ME_0162"); 
                return false;
            }

            double MaxQty = Convert.ToDouble(DataTableConverter.GetValue(dgBatchList.Rows[dgBatchList.SelectedIndex].DataItem, "MAX_QTY"));
            double SumOutputQty = Convert.ToDouble(DataTableConverter.GetValue(dgBatchList.Rows[dgBatchList.SelectedIndex].DataItem, "SUM_OUTPUT_QTY"));
            double ActQty = ACT_QTY.Value;
            if (MaxQty < SumOutputQty + ActQty)
            {
                Util.MessageValidation("SFU1145");
                return false;
            }


            return true;
        }

        private void btnPopup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CMM_ELEC_COATER_MANUAL_DRANE popup = new CMM_ELEC_COATER_MANUAL_DRANE { FrameOperation = FrameOperation };

                if (popup != null)
                {
                    object[] Parameters = new object[10];
                    //Parameters[0] = Util.GetCondition(cboAreaSupply);
                    //Parameters[1] = "A1ECOT001";
                    //popupSlurryMove.Closed += new EventHandler(PopupRollMapUpdate_Closed);
                    C1WindowExtension.SetParameters(popup, Parameters);

                    if (popup != null)
                    {
                        popup.ShowModal();
                        popup.CenterOnScreen();
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region User Method

        private void SetEquipmentMultiCombo(C1ComboBox cbArea, MultiSelectionBox mcb)
        {
            try
            {
                if (mcb.IsVisible == false) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = "E2000";     //코터 공정으로 고정
                dr["AREAID"] = Util.GetCondition(cbArea, "SFU3203");

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {

                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
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

        private void SetCommonCodeMultiCombo(String CMCDTYPE, MultiSelectionBox mcb)
        {
            try
            {
                if (mcb.IsVisible == false) return; // 컨트롤 생성되기 전에는 건너뛰기

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["EQPTID"] = cboEqptSearch.SelectedItemsToString ?? null;
                RQSTDT.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CT_SLURRY_MOUNT_PSTN_CBO_RM", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
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

        private void SetEquipmentCombo()
        {
            try
            {

                cboEqpt.ItemsSource = null;

                const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_AREA_CBO";
                string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
                string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.ToString(), "E2000" };
                string selectedValueText = "CBO_CODE";
                string displayMemberText = "CBO_NAME";

                CommonCombo.CommonBaseCombo(bizRuleName, cboEqpt, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEqptPstnComboBox()
        {
            try
            {
                cboEqptPstnID.ItemsSource = null;
                const string bizRuleName = "DA_PRD_SEL_CT_SLURRY_MOUNT_PSTN_CBO_RM";
                string[] arrColumn = { "LANGID", "AREAID", "EQPTID" };
                string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.ToString(), cboEqpt.SelectedValue.ToString() };
                string selectedValueText = "CBO_CODE";
                string displayMemberText = "CBO_NAME";

                CommonCombo.CommonBaseCombo(bizRuleName, cboEqptPstnID, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetBatchList(string sTargetLotID = null)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                ClearDlg();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                RQSTDT.Columns.Add("FROMDT", typeof(string));
                RQSTDT.Columns.Add("TODT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                //dr["EQPTID"] = cboEqpt.SelectedItemsToString;
                //dr["EQPT_MOUNT_PSTN_ID"] = cboEqptMeasrPstn.SelectedItemsToString;

                dr["FROMDT"] = Util.GetCondition(dtFrom);
                dr["TODT"] = Util.GetCondition(dtTo);

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXER__BATCH_DRAIN_LIST_CT", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgBatchList, dtRslt, null, true);

                if (!string.IsNullOrEmpty(sTargetLotID))
                {
                    for (int iRow = 0; iRow < dgBatchList.Rows.Count; iRow++)
                    {
                        if (sTargetLotID.Equals(Util.NVC(DataTableConverter.GetValue(dgBatchList.Rows[iRow].DataItem, "INPUT_LOTID"))))
                        {
                            dgBatchList.SelectedIndex = iRow;
                            DataTableConverter.SetValue(dgBatchList.Rows[iRow].DataItem, "CHK", 1);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveData()
        {
            loadingIndicator.Visibility = Visibility.Visible;

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("SRCTYPE", typeof(string));
            dtRqst.Columns.Add("EQPID", typeof(string));
            dtRqst.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            dtRqst.Columns.Add("LOTID", typeof(string));
            dtRqst.Columns.Add("ACT_QTY", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("MTRLID", typeof(string));

            DataRow Indata = dtRqst.NewRow();
            Indata["SRCTYPE"] = "UI";
            Indata["EQPID"] = cboEqpt.SelectedValue.ToString();
            Indata["EQPT_MOUNT_PSTN_ID"] = cboEqptPstnID.SelectedValue.ToString();
            Indata["LOTID"] = INPUT_LOTID.Text;
            Indata["ACT_QTY"] = ACT_QTY.Value;
            Indata["USERID"] = LoginInfo.USERID;
            Indata["MTRLID"] = _MTRLID;
            dtRqst.Rows.Add(Indata);            


            new ClientProxy().ExecuteService("BR_PRD_REG_MANUAL_BATCH_CT", "INDATA", null, dtRqst, (bizResult, bizException) =>
            {
                try
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageValidation("FM_ME_0239"); //SFU1595
                    GetBatchList();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
            });
        }

        private void ClearDlg()
        {
            INPUT_LOTID.Text = "";
            ACT_QTY.Value = 0;
            _MTRLID = "";

        }



        #endregion


    }
}
