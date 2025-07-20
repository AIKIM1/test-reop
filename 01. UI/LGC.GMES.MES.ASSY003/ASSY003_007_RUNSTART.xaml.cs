/*************************************************************************************
 Created Date : 2017.06.15
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Packaging 공정진척 화면 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.12  INS 김동일K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_007_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_007_RUNSTART : C1Window, IWorkArea
    {        
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _WoDetlId = string.Empty;
        private string _WoId = string.Empty;
        private string _processId = string.Empty;

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

        public ASSY003_007_RUNSTART()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // 생산 일자 Combo
            String[] sFilter = { "DATE_TYPE" };
            _combo.SetCombo(cboDay, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODE");

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

            if (tmps != null && tmps.Length >= 3)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _processId = Util.NVC(tmps[2]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
            }

            grdMsg.Visibility = Visibility.Collapsed;

            ApplyPermissions();

            GetEqptInfo();
            GetPkgMdlLotInfo();
            GetWaitBasket();
            GetInputInfo();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunStart())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업시작 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string sNewLot = GetNewLotid();
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

        private void dgWaitList_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    if (bSave)
                                        return;

                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    SetInput(Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LOTID")),
                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPQTY")),
                                                Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "PRODID")));

                                    for (int idx = 0; idx < dg.Rows.Count; idx++)
                                    {
                                        if (e.Cell.Row.Index != idx)
                                        {
                                            if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                            {
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                            }
                                            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                        }
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    if (bSave)
                                        return;

                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;

                                    for (int i = 0; i < dgInList.Rows.Count - dgInList.BottomRows.Count; i++)
                                    {
                                        if (!Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LOTID")).Equals("") &&
                                            Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LOTID")).Equals(Util.NVC(DataTableConverter.GetValue(dgInList.Rows[i].DataItem, "SEL_LOTID"))))
                                        {
                                            if (dgInList.Columns.Contains("SEL_LOTID"))
                                                DataTableConverter.SetValue(dgInList.Rows[i].DataItem, "SEL_LOTID", "");
                                            if (dgInList.Columns.Contains("INPUT_QTY"))
                                                DataTableConverter.SetValue(dgInList.Rows[i].DataItem, "INPUT_QTY", "0");
                                            if (dgInList.Columns.Contains("PRODID"))
                                                DataTableConverter.SetValue(dgInList.Rows[i].DataItem, "PRODID", "");
                                            break;
                                        }
                                    }
                                }
                                break;
                        }

                        if (dg.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg.Rows.Count > 0)
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                    }
                }
            }));
        }

        private void txtMTRL_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtMTRL.Text.Trim().Equals(""))
                        return;

                    GetWaitBasket_Info(txtMTRL.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetWaitBasket_Info(string slotid)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processId;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = slotid;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_CL_S", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            SetInput(Util.NVC(searchResult.Rows[0]["LOTID"]), Util.NVC(searchResult.Rows[0]["WIPQTY"]), Util.NVC(searchResult.Rows[0]["PRODID"]));
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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void dgInList_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    for (int idx = 0; idx < dg.Rows.Count; idx++)
                                    {
                                        if (e.Cell.Row.Index != idx)
                                        {
                                            if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                            {
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                            }
                                            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                        }
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;
                                }
                                break;
                        }

                        if (dg.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg.Rows.Count > 0)
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                    }
                }
            }));
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
                    _WoId = Util.NVC(dtRslt.Rows[0]["WOID"]);
                    _WoDetlId = Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]);
                }
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        private void GetWaitBasket()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processId;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;
                newRow["WO_DETL_ID"] = _WoDetlId;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_CL_S", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgWaitList.CurrentCellChanged -= dgWaitList_CurrentCellChanged;
                        Util.GridSetData(dgWaitList, searchResult, null);
                        dgWaitList.CurrentCellChanged += dgWaitList_CurrentCellChanged;


                        if (dgWaitList.CurrentCell != null)
                            dgWaitList.CurrentCell = dgWaitList.GetCell(dgWaitList.CurrentCell.Row.Index, dgWaitList.Columns.Count - 1);
                        else if (dgWaitList.Rows.Count > 0)
                            dgWaitList.CurrentCell = dgWaitList.GetCell(dgWaitList.Rows.Count, dgWaitList.Columns.Count - 1);
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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetNewLotid()
        {
            try
            {
                ShowLoadingIndicator();
                // 랏 채번..
                DataSet indataSet = _Biz.GetBR_PRD_GET_REQLOT_CL();
                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                if (_processId != Process.SSC_FOLDED_BICELL)
                {
                    newRow["NEXT_DAY_WORK"] = cboDay.SelectedValue != null && cboDay.SelectedValue.ToString().Equals("TOMORROW") ? "Y" : "N";
                }
                //newRow["NEXT_DAY_WORK"] = cboDay.SelectedValue != null && cboDay.SelectedValue.ToString().Equals("1") ? "Y" : "N";
                newRow["REQTYPE"] = null;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                //DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                //newRow = inMtrlTable.NewRow();
                //newRow["INPUT_LOTID"] = txtPackagingLot.Text;
                //newRow["MTRLID"] = "";
                //newRow["EQPT_MOUNT_PSTN_ID"] = "";
                //newRow["EQPT_MOUNT_PSTN_STATE"] = "";

                //inMtrlTable.Rows.Add(newRow);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_NEW_PROD_LOTID_CL_S", "IN_EQP,IN_INPUT", "OUTDATA", indataSet);

                string sNewLot = string.Empty;
                if (CommonVerify.HasTableInDataSet(dsRslt))
                {
                    if (CommonVerify.HasTableRow(dsRslt.Tables["OUTDATA"]))
                    {
                        sNewLot = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["PROD_LOTID"]);

                        txtPackagingLot.Text = sNewLot;
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

        private void GetPkgMdlLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_MDLLOT_INFO_CL();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["WOID"] = _WoId;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MDLLOT_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    txtModel.Text = Util.NVC(dtRslt.Rows[0]["MDLLOT_ID"]);
                    //txtElecFrom.Text = Util.NVC(dtRslt.Rows[0]["ELEC_PROD_SITE"]);
                }
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void RunStart(string sNewLot)
        {
            try
            {
                ShowLoadingIndicator();
                // 착공 처리..
                DataSet indataSet = _Biz.GetBR_PRD_REG_LOTSTART_CL();
                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = sNewLot;
                newRow["WO_DETL_ID"] = _processId == Process.SSC_FOLDED_BICELL ? _WoDetlId : null;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgInList.Rows.Count - dgInList.BottomRows.Count; i++)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgInList.Rows[i].DataItem, "SEL_LOTID")).Equals(""))
                    {
                        newRow = inMtrlTable.NewRow();
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInList.Rows[i].DataItem, "SEL_LOTID"));
                        newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgInList.Rows[i].DataItem, "PRODID"));
                        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInList.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";

                        inMtrlTable.Rows.Add(newRow);
                    }
                    //else
                    //{
                    //    if (!Util.NVC(DataTableConverter.GetValue(dgInList.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                    //    {
                    //        newRow = inMtrlTable.NewRow();
                    //        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInList.Rows[i].DataItem, "INPUT_LOTID"));
                    //        newRow["MTRLID"] = "";
                    //        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInList.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    //        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";

                    //        inMtrlTable.Rows.Add(newRow);
                    //    }
                    //}
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_LOT_CL_S", "IN_EQP,IN_INPUT", null, indataSet);

                // 버튼 disable.. 
                dgWaitList.IsReadOnly = true;
                dgInList.IsReadOnly = true;
                //cboDay.IsEnabled = false;
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

        private void GetInputInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_INPUT_POS_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;
                newRow["MOUNT_MTRL_TYPE_CODE"] = "PROD"; // 바구니 투입위치만 조회.

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_MOUNT_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgInList.CurrentCellChanged -= dgInList_CurrentCellChanged;
                        //dgInList.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInList, searchResult, null);
                        dgInList.CurrentCellChanged += dgInList_CurrentCellChanged;

                        if (dgInList != null && dgInList.Rows.Count == 1)
                        {
                            DataTableConverter.SetValue(dgInList.Rows[0].DataItem, "CHK", true);
                        }

                        if (dgInList.CurrentCell != null)
                            dgInList.CurrentCell = dgInList.GetCell(dgInList.CurrentCell.Row.Index, dgInList.Columns.Count - 1);
                        else if (dgInList.Rows.Count > 0)
                            dgInList.CurrentCell = dgInList.GetCell(dgInList.Rows.Count, dgInList.Columns.Count - 1);
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
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

        private void SetInput(string sLot, string sQty, string sProdid)
        {
            if (dgInList.ItemsSource == null || dgInList.Rows.Count <= 0)
                return;

            int iRow = -1;

            if (!sLot.Equals(""))
            {
                iRow = _Util.GetDataGridRowIndex(dgInList, "INPUT_LOTID", sLot);
                if (iRow >= 0)
                {
                    //Util.Alert("투입LOT에 동일한 LOT이 있습니다.");
                    Util.MessageValidation("SFU1967");
                    return;
                }
            }

            if (!sLot.Equals(""))
            {
                iRow = _Util.GetDataGridRowIndex(dgInList, "SEL_LOTID", sLot);
                if (iRow >= 0)
                {
                    //Util.Alert("선택한 LOT에 동일한 LOT이 있습니다.");
                    Util.MessageValidation("SFU1657");
                    return;
                }
            }

            iRow = _Util.GetDataGridCheckFirstRowIndex(dgInList, "CHK");

            if (iRow < 0)
            {
                //Util.Alert("투입위치를 선택 하세요.");
                Util.MessageValidation("SFU1981");
                return;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgInList.Rows[iRow].DataItem, "INPUT_LOTID")).Trim().Equals(""))
            {
                //Util.Alert("해당 위치는 이미 투입 정보가 존재하여 투입할 수 없습니다.");
                Util.MessageValidation("SFU2021");
                return;
            }

            DataTable dtTmp = DataTableConverter.Convert(dgInList.ItemsSource);

            if (!dtTmp.Columns.Contains("INPUT_LOTID"))
                dtTmp.Columns.Add("INPUT_LOTID", typeof(string));
            if (!dtTmp.Columns.Contains("INPUT_QTY"))
                dtTmp.Columns.Add("INPUT_QTY", typeof(int));
            if (!dtTmp.Columns.Contains("PRODID"))
                dtTmp.Columns.Add("PRODID", typeof(string));
            if (!dtTmp.Columns.Contains("SEL_LOTID"))
                dtTmp.Columns.Add("SEL_LOTID", typeof(string));

            dtTmp.Rows[iRow]["SEL_LOTID"] = sLot;
            dtTmp.Rows[iRow]["INPUT_QTY"] = Convert.ToDecimal(sQty);
            dtTmp.Rows[iRow]["PRODID"] = sProdid;

            dgInList.BeginEdit();
            dgInList.ItemsSource = DataTableConverter.Convert(dtTmp);
            dgInList.EndEdit();
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
