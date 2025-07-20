/*************************************************************************************
 Created Date : 2023.12.15
         Creator : 조성근
      Decription : Coater Manual Drane
--------------------------------------------------------------------------------------
 [Change History]
  2023.12.15 조성근 : 최초생성
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
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

    public partial class CMM_ELEC_COATER_MANUAL_DRANE : C1Window, IWorkArea
    {
        #region Declare Variable
        private string _AREAID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTPSTNID = string.Empty;


        bool bFinishLoading = false;


        CommonCombo _combo = new CommonCombo();

        public CMM_ELEC_COATER_MANUAL_DRANE()
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

                SetEquipmentCombo();

                if (_EQPTID != "") cboEqpt.SelectedValue = _EQPTID;

                SetEqptPstnComboBox();

                if (_EQPTPSTNID != "") cboEqptPstnID.SelectedValue = _EQPTPSTNID;

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

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].IsFalse())
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

                    DRAIN_LOTID.Text = DataTableConverter.GetValue(dgBatchList.Rows[idx].DataItem, "INPUT_LOTID").ToString();
                    INPUT_QTY.Value = Convert.ToDouble(DataTableConverter.GetValue(dgBatchList.Rows[idx].DataItem, "INPUT_QTY"));
                    REMAIN_QTY.Value = Convert.ToDouble(DataTableConverter.GetValue(dgBatchList.Rows[idx].DataItem, "REMAIN_QTY"));
                    ACT_QTY.Value = Convert.ToDouble(DataTableConverter.GetValue(dgBatchList.Rows[idx].DataItem, "REMAIN_QTY"));

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

            SetEquipmentCombo();
        }

        private void cboEqpt_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bFinishLoading == false) return;
            if (cboEqpt.SelectedIndex < 0) return;

            SetEqptPstnComboBox();
        }

        private void cboEqptPstnID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bFinishLoading == false) return;
            if (cboEqptPstnID.SelectedIndex < 0) return;

            GetBatchList();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetBatchList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (checkValues() == false) return;
            Util.MessageConfirm("SFU3613", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveData();
                }
            });            
        }

        private bool checkValues()
        {
            if (DRAIN_LOTID.Text == "")
            {
                Util.MessageValidation("FM_ME_0162");
                return false;
            }

            return true;
        }

        private void btnPopup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CMM_ELEC_COATER_MANUAL_INPUT popup = new CMM_ELEC_COATER_MANUAL_INPUT { FrameOperation = FrameOperation };

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

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                DataRow Indata = dtRqst.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = cboEqpt.SelectedValue.ToString();
                Indata["EQPT_MOUNT_PSTN_ID"] = cboEqptPstnID.SelectedIndex == -1 ? null:cboEqptPstnID.SelectedValue.ToString();

                dtRqst.Rows.Add(Indata);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BATCH_LST_CT", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgBatchList, dtRslt, null, true);

                if (!string.IsNullOrEmpty(sTargetLotID))
                {
                    for(int iRow = 0; iRow < dgBatchList.Rows.Count ; iRow++)
                    {
                        if(sTargetLotID.Equals(Util.NVC(DataTableConverter.GetValue(dgBatchList.Rows[iRow].DataItem, "INPUT_LOTID"))))
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

            DataRow Indata = dtRqst.NewRow();
            Indata["SRCTYPE"] = "UI";
            Indata["EQPID"] = cboEqpt.SelectedValue.ToString();
            Indata["EQPT_MOUNT_PSTN_ID"] = cboEqptPstnID.SelectedValue.ToString();
            Indata["LOTID"] = DRAIN_LOTID.Text;
            Indata["ACT_QTY"] = ACT_QTY.Value;
            Indata["USERID"] = LoginInfo.USERID;
            dtRqst.Rows.Add(Indata);


            new ClientProxy().ExecuteService("BR_PRD_REG_MIXER_DRAIN_CT", "INDATA", null, dtRqst, (bizResult, bizException) =>
            {
                try
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageValidation("FM_ME_0239"); 
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
            DRAIN_LOTID.Text = "";
            INPUT_QTY.Value = 0;
            REMAIN_QTY.Value = 0;
            ACT_QTY.Value = 0;

    }




        #endregion

        
    }
}
