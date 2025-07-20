/*************************************************************************************
 Created Date : 2022.02.10
      Creator : INS 김동일
   Decription : CT공정 - 작업시작
--------------------------------------------------------------------------------------
 [Change History]
  2022.02.10  INS 김동일 : Initial Created.

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
    /// ASSY001_011_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_011_RUNSTART : C1Window, IWorkArea
    {   
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _procid = string.Empty;
        //private string _EqptMountPstn = string.Empty;

        public string NEW_PROD_LOT = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        private bool bSave = false;
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

        public ASSY001_011_RUNSTART()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter1 = { _EqptID, "PROD" };
            _combo.SetCombo(cboBoxMountPstsID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
        }

        #endregion

        #region Event

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            //InitCombo();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _LineID = Util.NVC(tmps[0]);
            _EqptID = Util.NVC(tmps[1]);
            _procid = Util.NVC(tmps[2]);

            grdMsg.Visibility = Visibility.Collapsed;

            ApplyPermissions();

            InitCombo();
            //GetEqptMountPstn();
        }

        private void tbxInLot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    ShowLoadingIndicator();

                    DataTable dt = new DataTable("INDATA");
                    dt.Columns.Add("LANGID");
                    dt.Columns.Add("SHOPID");
                    dt.Columns.Add("AREAID");
                    dt.Columns.Add("LOTID");

                    DataRow dr = dt.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["LOTID"] = tbxInLot.Text.Trim();
                    dt.Rows.Add(dr);

                    new ClientProxy().ExecuteService("BR_PRD_SEL_WAIT_LOT_LIST_CI", "INDATA", "OUTDATA", dt, (resultDt, exception) =>
                    {
                        try
                        {
                            ShowLoadingIndicator();

                            if (exception != null)
                            {
                                Util.MessageException(exception);
                                return;
                            }

                            Util.GridSetData(dgdLotInfo, resultDt, FrameOperation, false);
                            tbxInLot.IsEnabled = false;
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
                    HiddenLoadingIndicator();
                }
            }
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
                    string sNewLot = GetNewCTINSPLotid();

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

        #endregion

        #region Mehod

        #region [BizRule]
        private string GetNewCTINSPLotid()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                //inTable.Columns.Add("LOT_MODE", typeof(string));
                //inTable.Columns.Add("AN_LOT_TYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                //newRow["LOT_MODE"] = Util.GetCondition(cboLotMode);
                //newRow["AN_LOT_TYPE"] = Util.GetCondition(cboAnLotType);
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_ID", typeof(string));

                newRow = inTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(cboBoxMountPstsID.SelectedValue);
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_ID"] = DataTableConverter.GetValue(dgdLotInfo.Rows[0].DataItem, "LOTID");
                inTable.Rows.Add(newRow);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_NEW_PROD_LOTID_CI", "IN_EQP,IN_INPUT", "OUT_LOT", indataSet);

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
                ShowLoadingIndicator();

                // 착공 처리..
                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                //inDataTable.Columns.Add("LOT_MODE", typeof(string));
                //inDataTable.Columns.Add("AN_LOT_TYPE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = sNewLot;
                //newRow["LOT_MODE"] = Util.GetCondition(cboLotMode);
                //newRow["AN_LOT_TYPE"] = Util.GetCondition(cboAnLotType);
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
                inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inMtrl.Columns.Add("INPUT_LOTID", typeof(string));

                inTable = indataSet.Tables["IN_INPUT"];
                newRow = inTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(cboBoxMountPstsID.SelectedValue);
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = DataTableConverter.GetValue(dgdLotInfo.Rows[0].DataItem, "LOTID");
                inTable.Rows.Add(newRow);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_PROD_LOT_CI", "IN_EQP,IN_INPUT", "OUT_LOT", indataSet);

                btnOK.IsEnabled = false;

                bSave = true;

                NEW_PROD_LOT = sNewLot;

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

        //private void GetEqptMountPstn()
        //{
        //    try
        //    {
        //        DataTable inTable = new DataTable();
        //        inTable.Columns.Add("EQPTID", typeof(string));

        //        DataRow newRow = inTable.NewRow();
        //        newRow["EQPTID"] = _EqptID;
        //        inTable.Rows.Add(newRow);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_IN_LOT_LIST_L", "INDATA", "OUTDATA", inTable);

        //        if (dtResult.Rows.Count < 1)
        //        {
        //            Util.MessageValidation("SFU2020"); //해당 설비의 장착위치부 정보가 없습니다.
        //            return;
        //        }
        //        else
        //        {
        //            if (dtResult.Rows[0]["MOUNT_MTRL_TYPE_CODE"].ToString().Equals("PROD"))
        //                _EqptMountPstn = dtResult.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
        //            else
        //            {
        //                Util.MessageValidation("SFU2020"); //해당 설비의 장착위치부 정보가 없습니다.
        //                return;
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        HiddenLoadingIndicator();
        //        Util.MessageException(ex);
        //    }
        //}

        #endregion

        #region [Validation]
        private bool CanRunStart()
        {
            bool bRet = false;

            if (cboBoxMountPstsID == null || cboBoxMountPstsID.SelectedValue == null || cboBoxMountPstsID.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU2020"); //해당 설비의 장착위치부 정보가 없습니다.
                return bRet;
            }

            if (Util.NVC(cboBoxMountPstsID.SelectedValue).IndexOf("SELECT") > -1)
            {
                Util.MessageValidation("SFU1981"); //투입위치를 선택하세요.
                return bRet;
            }

            if (dgdLotInfo?.Rows?.Count < 1)
            {
                Util.MessageValidation("SFU1945"); //투입 LOT이 없습니다.
                return bRet;
            }

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
