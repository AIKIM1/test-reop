/*************************************************************************************
 Created Date : 2017.11.01
      Creator : 고현영
   Decription : GMES - 패키징(소형) 공정진척 - C생산BOX인수
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.01  고현영: Initial Created.
  2017.12.09  고현영: 탭추가 및 화면수정
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Net;
using System.Reflection;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.ASSY003
{
    public partial class ASSY003_007_BOX_IN : C1Window, IWorkArea
    {
        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
        private string _ProcId = string.Empty;
        private string _EqptId = string.Empty;
        private string _EqsgId = string.Empty;
        private string _Prod_WoId = string.Empty;

        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY003_007_BOX_IN()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //재작업 유형 Combo
            String[] sFilter1 = { "CPROD_WRK_TYPE_CODE" };
            _combo.SetCombo(cboWrkType_tabCProdIn, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
            
            _combo.SetCombo(cboWrkType_tabHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            if (_ProcId.Equals(Process.STACKING_FOLDING))
            {
                cboWrkType_tabCProdIn.IsEnabled = false;
                cboWrkType_tabHist.IsEnabled = false;
            }
        }

        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 3)
            {
                _EqsgId = Util.NVC(tmps[0]);
                _EqptId = Util.NVC(tmps[1]);
                _ProcId = Util.NVC(tmps[2]);
            }
            else
            {
                _EqsgId = "";
                _EqptId = "";
                _ProcId = "";
            }

            ApplyPermissions();

            //InitializeControls();

            //ClearControls();

            InitCombo();

            //SetControls();
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnInCProd);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        
        private bool CanBoxIn()
        {
            bool bRet = false;

            if(dgdLotList.Rows.Count <= 0)
            {
                //입력된 LOT ID 데이터가 없습니다.
                Util.MessageValidation("SFU1009");
                return bRet;
            }

            if (Util.NVC(txtUserName.Text).Equals(""))
            {
                Util.MessageValidation("SFU4011");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void BoxInProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("MOVE_ORD_ID", typeof(string));
                inDataTable.Columns.Add("RCPT_USERID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptId;
                newRow["MOVE_ORD_ID"] = Util.NVC(DataTableConverter.GetValue(dgdTransLotList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgdTransLotList, "CHK")].DataItem, "MOVE_ORD_ID"));
                newRow["RCPT_USERID"] = Util.NVC(txtUserName.Tag);
                newRow["NOTE"] = "";
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);

                for (int i = 0; i < dgdLotList.Rows.Count; i++)
                {
                    newRow = inLot.NewRow();

                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgdLotList.Rows[i].DataItem, "LOTID"));

                    inLot.Rows.Add(newRow);
                }
                
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_CPROD_OUT_LOT", "INDATA,IN_LOT", "OUTDATA", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        SearchMovingMst();

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnPerson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CMM_PERSON wndPerson = new CMM_PERSON();
                wndPerson.FrameOperation = this.FrameOperation;

                if (wndPerson != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = txtUserName.Text;

                    C1WindowExtension.SetParameters(wndPerson, Parameters);

                    wndPerson.Closed += new EventHandler(wndPerson_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndPerson_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPopup = sender as CMM_PERSON;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPopup.USERNAME;
                txtUserName.Tag = wndPopup.USERID;
            }
        }

        private void btnSearch_taCProdIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchMovingMst();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchMovingMst()
        {
            try
            {
                ShowLoadingIndicator();

                Util.gridClear(dgdLotList);

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("TO_PROCID", typeof(string));
                inTable.Columns.Add("TO_EQSGID", typeof(string));
                inTable.Columns.Add("MOVE_ORD_ID", typeof(string));
                inTable.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(string));
                inTable.Columns.Add("FROM_PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["TO_PROCID"] = _ProcId;
                newRow["TO_EQSGID"] = _EqsgId;
                newRow["MOVE_ORD_ID"] = tbxCProdLot.Text.Trim().Equals("") ? null : tbxCProdLot.Text.Trim();
                newRow["CPROD_WRK_TYPE_CODE"] = Util.NVC(cboWrkType_tabCProdIn.SelectedValue).Equals("") ? null : Util.NVC(cboWrkType_tabCProdIn.SelectedValue);
                newRow["FROM_PROCID"] = Process.CPROD;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_REWORK_MOVING_MST_LIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgdTransLotList, searchResult, FrameOperation, false);
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
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private void SearchMovingDtl(string sMoveOrderID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MOVE_ORD_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MOVE_ORD_ID"] = sMoveOrderID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_REWORK_MOVING_DTL_LIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgdLotList, searchResult, FrameOperation, false);
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
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private void dgdTransLotList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
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

                                int preValue = (int)e.Cell.Value;

                                Util.DataGridCheckAllUnChecked(dg);

                                if (preValue > 0)
                                {
                                    e.Cell.Value = true;
                                    SearchMovingDtl(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MOVE_ORD_ID")));
                                }
                                else e.Cell.Value = false;                                

                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInCProd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanBoxIn()) return;

                // 인수 하시겠습니까?
                Util.MessageConfirm("SFU4273", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        BoxInProcess();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_tabBoxOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchMovingMstHist();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchMovingMstHist()
        {
            try
            {
                ShowLoadingIndicator();

                Util.gridClear(dgdLotList);

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("TO_PROCID", typeof(string));
                inTable.Columns.Add("TO_EQSGID", typeof(string));
                inTable.Columns.Add("MOVE_ORD_ID", typeof(string));
                inTable.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(string));
                inTable.Columns.Add("FROM_PROCID", typeof(string));
                inTable.Columns.Add("FRDT", typeof(DateTime));
                inTable.Columns.Add("TODT", typeof(DateTime));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["TO_PROCID"] = _ProcId;
                newRow["TO_EQSGID"] = _EqsgId;
                newRow["MOVE_ORD_ID"] = tbxCProdLot.Text.Trim().Equals("") ? null : tbxCProdLot.Text.Trim();
                newRow["CPROD_WRK_TYPE_CODE"] = Util.NVC(cboWrkType_tabCProdIn.SelectedValue).Equals("") ? null : Util.NVC(cboWrkType_tabCProdIn.SelectedValue);
                newRow["FROM_PROCID"] = Process.CPROD;
                newRow["FRDT"] = dtpFrom_tabCProdInHist.SelectedDateTime;
                newRow["TODT"] = dtpTo_tabCProdInHist.SelectedDateTime;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_REWORK_MOVING_MST_HIST_LIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgdTransWaiting_tabBoxOut, searchResult, FrameOperation, false);
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
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private void dgdTransWaiting_tabBoxOut_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
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

                                int preValue = (int)e.Cell.Value;

                                Util.DataGridCheckAllUnChecked(dg);

                                if (preValue > 0)
                                {
                                    e.Cell.Value = true;
                                    SearchMovingDtlHist(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MOVE_ORD_ID")));
                                }
                                else e.Cell.Value = false;

                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchMovingDtlHist(string sMoveOrderID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MOVE_ORD_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MOVE_ORD_ID"] = sMoveOrderID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_REWORK_MOVING_DTL_LIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgdTransList_tabBoxOut, searchResult, FrameOperation, false);
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
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

    }
}
