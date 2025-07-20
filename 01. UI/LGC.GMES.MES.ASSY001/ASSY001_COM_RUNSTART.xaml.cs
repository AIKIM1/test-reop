/*************************************************************************************
 Created Date : 2019.05.29
      Creator : INS 김동일K
   Decription : CWA3동 증설 - 조립 공정 공통 - 작업시작
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.29  INS 김동일K : Initial Created.
  2023.12.08  안유수 : E20231005-000815 설비 투입자재 정보 누락시 Interlock 기능 구현

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_COM_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_COM_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _procid = string.Empty;

        public string NEW_PROD_LOT = string.Empty;
        
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        private bool bSave = false;

        private bool _Mtrl_InterLock_Use_Eqpt_YN = false;
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_COM_RUNSTART()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { "PROD_LOT_OPER_MODE" };
            _combo.SetCombo(cboLotMode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODE");

            if (cboLotMode.Items.Count > 0)
                cboLotMode.SelectedValue = "L"; // UI 는 비정규 모드.

            String[] sFilter2 = { "IRREGL_PROD_LOT_TYPE_CODE" };
            _combo.SetCombo(cboAnLotType, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "COMMCODE");

            // 생산 일자 Combo
            String[] sFilter3 = { "DATE_TYPE" };
            _combo.SetCombo(cboDay, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");

            if (cboDay != null && cboDay.Items != null && cboDay.Items.Count > 0)
                cboDay.SelectedIndex = 0;
        }

        #endregion

        #region Event

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _LineID = Util.NVC(tmps[0]);
            _EqptID = Util.NVC(tmps[1]);
            _procid = Util.NVC(tmps[2]);

            grdMsg.Visibility = Visibility.Collapsed;

            ApplyPermissions();

            GetEqptInfo();

            if (string.Equals(_procid, Process.PACKAGING))
                cboDay.Visibility = Visibility.Visible;
            else
                cboDay.Visibility = Visibility.Collapsed;

            GetCurrMountInfo();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunStart())
                return;

            // 작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string sNewLot = string.Empty;

                    if (string.Equals(_procid, Process.LAMINATION))
                        sNewLot = GetNewLamLotId();
                    else if (string.Equals(_procid, Process.STACKING_FOLDING))
                        sNewLot = GetNewFolLotId();
                    else if (string.Equals(_procid, Process.PACKAGING))
                        sNewLot = GetNewPkgLotid();
                    
                    if (sNewLot.Equals(""))
                        return;

                    RunStart(sNewLot);
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (bSave)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                        else
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                    }

                    dgInputMtrl.SelectedIndex = idx;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtInputLotID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (!_Mtrl_InterLock_Use_Eqpt_YN)
                    return;

                if (e.Key == Key.Enter)
                {
                    // 권한 없으면 Skip.
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                        return;

                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgInputMtrl, "CHK");

                    if (idx < 0)
                    {
                        Util.MessageValidation("SFU1981");
                        txtInputLotID.Text = "";
                        return;
                    }

                    if (txtInputLotID.Text.Trim().Equals(""))
                    {
                        return;
                    }

                    DataRow dtRow = (dgInputMtrl.Rows[idx].DataItem as DataRowView).Row;

                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("{0} 위치에 {1} 을 투입 하시겠습니까?", sInPosName, txtCurrInLotID.Text.Trim()), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    object[] parameters = new object[2];
                    parameters[0] = dtRow["EQPT_MOUNT_PSTN_NAME"].ToString();
                    parameters[1] = txtInputLotID.Text.Trim();

                    Util.MessageConfirm("SFU1291", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgInputMtrl.Rows[idx].DataItem, "INPUT_LOTID", txtInputLotID.Text.Trim());
                            DataTableConverter.SetValue(dgInputMtrl.Rows[idx].DataItem, "FLAG", "Y");

                            txtInputLotID.Text = "";
                        }
                    }, parameters);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                txtInputLotID.Text = "";
            }
        }

        private void txtInputLotID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtInputLotID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtInputLotID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        #region [BizRule]
        private void GetEqptInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    txtWorkorder.Text = Util.NVC(dtRslt.Rows[0]["WOID"]);
                    txtWODetail.Text = Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]);                    
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
        }

        private string GetNewLamLotId()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();
                
                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOT_MODE", typeof(string));
                inTable.Columns.Add("AN_LOT_TYPE", typeof(string));
                inTable.Columns.Add("LAMI_CELL_TYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["LOT_MODE"] = Util.GetCondition(cboLotMode);
                newRow["AN_LOT_TYPE"] = Util.GetCondition(cboAnLotType);
                //newRow["LAMI_CELL_TYPE"] = "";
                newRow["USERID"] = LoginInfo.USERID;
                
                inTable.Rows.Add(newRow);
                newRow = null;

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_NEW_PROD_LOTID_LM_L", "IN_EQP,IN_INPUT", "OUT_LOT", indataSet);

                string sNewLot = string.Empty;
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    if (CommonVerify.HasTableRow(dsResult.Tables["OUT_LOT"]))
                    {
                        sNewLot = Util.NVC(dsResult.Tables["OUT_LOT"].Rows[0]["PROD_LOTID"]);
                    }
                }

                return sNewLot;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private string GetNewFolLotId()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOT_MODE", typeof(string));
                inTable.Columns.Add("AN_LOT_TYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["LOT_MODE"] = Util.GetCondition(cboLotMode);
                newRow["AN_LOT_TYPE"] = Util.GetCondition(cboAnLotType);
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;
                
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_NEW_PROD_LOTID_FD_L", "IN_EQP,IN_INPUT", "OUTDATA", indataSet);

                string sNewLot = string.Empty;                
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    if (CommonVerify.HasTableRow(dsResult.Tables["OUTDATA"]))
                    {
                        sNewLot = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["PROD_LOTID"]);
                    }
                }

                return sNewLot;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private string GetNewPkgLotid()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("NEXT_DAY_WORK", typeof(string)); 
                inTable.Columns.Add("LOT_MODE", typeof(string));
                inTable.Columns.Add("AN_LOT_TYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["NEXT_DAY_WORK"] = cboDay.SelectedValue != null && cboDay.SelectedValue.ToString().Equals("TOMORROW") ? "Y" : "N";
                newRow["LOT_MODE"] = Util.GetCondition(cboLotMode);
                newRow["AN_LOT_TYPE"] = Util.GetCondition(cboAnLotType);
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;
                
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_NEW_PROD_LOTID_CL_L", "IN_EQP,IN_INPUT", "OUT_LOT", indataSet);

                string sNewLot = string.Empty;
                if (CommonVerify.HasTableInDataSet(dsRslt))
                {
                    if (CommonVerify.HasTableRow(dsRslt.Tables["OUT_LOT"]))
                    {
                        sNewLot = Util.NVC(dsRslt.Tables["OUT_LOT"].Rows[0]["PROD_LOTID"]);
                    }
                }

                HiddenLoadingIndicator();

                return sNewLot;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return "";
            }
        }
                
        private void RunStart(string sNewLot)
        {
            try
            {
                string sBizName = string.Empty;
                string sRetDataSet = string.Empty;

                if (string.Equals(_procid, Process.LAMINATION))
                {
                    sBizName = "BR_PRD_REG_START_PROD_LOT_LM_L";
                    sRetDataSet = "OUT_LOT";
                }
                else if (string.Equals(_procid, Process.STACKING_FOLDING))
                {
                    sBizName = "BR_PRD_REG_START_PROD_LOT_FD_L";
                    sRetDataSet = "OUT_LOT";
                }
                else if (string.Equals(_procid, Process.PACKAGING))
                {
                    sBizName = "BR_PRD_REG_START_PROD_LOT_CL_L";
                    sRetDataSet = "";
                }
                
                ShowLoadingIndicator();
                
                // 착공 처리..
                DataSet indataSet = new DataSet();
               
                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
                inDataTable.Columns.Add("LOT_MODE", typeof(string));
                inDataTable.Columns.Add("AN_LOT_TYPE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");                
                inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
                inMtrl.Columns.Add("CSTID", typeof(string));

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = sNewLot;
                //newRow["WO_DETL_ID"] = "";
                newRow["LOT_MODE"] = Util.GetCondition(cboLotMode);
                newRow["AN_LOT_TYPE"] = Util.GetCondition(cboAnLotType);
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                if (_Mtrl_InterLock_Use_Eqpt_YN)
                {
                    newRow = null;

                    DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];

                    for (int i = 0; i < dgInputMtrl.Rows.Count - dgInputMtrl.BottomRows.Count; i++)
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgInputMtrl.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                        //&& Util.NVC(DataTableConverter.GetValue(dgInputMtrl.Rows[i].DataItem, "FLAG")).Equals("Y"))
                        {
                            string sPstnState = Util.NVC(DataTableConverter.GetValue(dgInputMtrl.Rows[i].DataItem, "MOUNT_PSTN_STAT_CODE"));

                            if (!(sPstnState.Equals("A") || sPstnState.Equals("S")))
                                sPstnState = "A";

                            newRow = inMtrlTable.NewRow();
                            newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputMtrl.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                            newRow["EQPT_MOUNT_PSTN_STATE"] = Util.NVC(DataTableConverter.GetValue(dgInputMtrl.Rows[i].DataItem, "FLAG")).Equals("Y") ? "A" : sPstnState;
                            newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputMtrl.Rows[i].DataItem, "INPUT_LOTID"));

                            inMtrlTable.Rows.Add(newRow);
                        }
                    }
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "IN_EQP,IN_INPUT", sRetDataSet.Equals("") ? null : sRetDataSet, indataSet);

                btnOK.IsEnabled = false;

                bSave = true;

                NEW_PROD_LOT = sNewLot;

                //this.DialogResult = MessageBoxResult.OK;

                HiddenLoadingIndicator();

                tbSplash.Text = MessageDic.Instance.GetMessage("SFU3044", sNewLot); // [%1] LOT이 생성 되었습니다.

                grdMsg.Visibility = Visibility.Visible;

                AsynchronousClose();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetCurrMountInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_ITL_LST", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    _Mtrl_InterLock_Use_Eqpt_YN = true;
                    Util.GridSetData(dgInputMtrl, dtRslt, FrameOperation, false);

                    grdMainContents.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    _Mtrl_InterLock_Use_Eqpt_YN = false;
                    grdMainContents.RowDefinitions[2].Height = new GridLength(0);
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
        }
        #endregion

        #region [Validation]
        private bool CanRunStart()
        {
            bool bRet = false;

            bRet = true;
            return bRet;
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOK);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        
        private void AsynchronousClose()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #endregion
    }
}
