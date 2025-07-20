/*************************************************************************************
 Created Date : 2017.11.01
      Creator : 고현영
   Decription : 폴딩 공정진척 정상라미셀 C생산으로 인계
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.01  고현영: Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.ASSY003
{
    public partial class ASSY003_005_CELL_TRANSFER : C1Window, IWorkArea
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

        public ASSY003_005_CELL_TRANSFER()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

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

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter1 = { LoginInfo.CFG_AREA_ID, null, Process.CPROD };
            C1ComboBox[] cboLineChild1 = { cboTransEqpt };
            _combo.SetCombo(cboTransEqsg, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild1, sCase: "cboEquipmentSegmentAssy", sFilter: sFilter1);
            
            String[] sFilter2 = { Process.CPROD };
            C1ComboBox[] cboEquipmentParent1 = { cboTransEqsg };
            _combo.SetCombo(cboTransEqpt, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent1, sCase: "EQUIPMENT_MAIN_LEVEL", sFilter: sFilter2);
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length == 2)
            {
                _EqsgId = Util.NVC(tmps[0]);
                _EqptId = Util.NVC(tmps[1]);
            }
            else
            {
                _EqsgId = "";
                _EqptId = "";
            }

            //ApplyPermissions();

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
            listAuth.Add(btnTransfer);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanTransfer())
                    return;

                // 인계 하시겠습니까?
                Util.MessageConfirm("SFU2931", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Transfer();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CanSearch()
        {
            bool bRet = false;

            string sLotid = string.Empty;
            sLotid = txtMagLotId.Text.Trim();

            if (sLotid == "")
            {
                Util.MessageValidation("SFU1813");   //입력한 LOTID가 없습니다.
                return bRet;
            }

            for (int i = 0; i < dgTransferMagList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTransferMagList.Rows[i].DataItem, "LOTID")).Equals(sLotid))
                {
                    Util.MessageValidation("SFU2014");   //해당 LOT이 이미 존재합니다.
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanTransfer()
        {
            bool bRet = false;

            if (dgTransferMagList.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1661"); 
                return bRet;
            }

            if (cboTransEqsg.SelectedIndex < 0 || cboTransEqsg.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboTransEqpt.SelectedIndex < 0 || cboTransEqpt.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
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

        private void Transfer()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet ds = new DataSet();

                DataTable dt = ds.Tables.Add("INDATA");
                dt.Columns.Add("SRCTYPE", typeof(string));
                dt.Columns.Add("IFMODE", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("TO_EQSGID", typeof(string));                
                dt.Columns.Add("TO_PROCID", typeof(string));
                dt.Columns.Add("TO_EQPTID", typeof(string));
                dt.Columns.Add("MOVE_USERID", typeof(string));
                dt.Columns.Add("NOTE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                
                DataTable dtLot = ds.Tables.Add("IN_LOT");
                dtLot.Columns.Add("LOTID", typeof(string));
                dtLot.Columns.Add("PRODID", typeof(string));
                dtLot.Columns.Add("MKT_TYPE_CODE", typeof(string));
                dtLot.Columns.Add("WIPQTY", typeof(decimal));
                dtLot.Columns.Add("CSTID", typeof(string));

                DataRow dtRow = dt.NewRow();
                dtRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dtRow["IFMODE"] = IFMODE.IFMODE_OFF;
                dtRow["EQPTID"] = _EqptId;
                dtRow["TO_EQSGID"] = Util.NVC(cboTransEqsg.SelectedValue);
                dtRow["TO_PROCID"] = Process.CPROD;
                dtRow["TO_EQPTID"] = Util.NVC(cboTransEqpt.SelectedValue);
                dtRow["MOVE_USERID"] = Util.NVC(txtUserName.Tag);
                dtRow["NOTE"] = "";
                dtRow["USERID"] = LoginInfo.USERID;

                dt.Rows.Add(dtRow);

                foreach (var row in dgTransferMagList.Rows)
                {
                    dtRow = dtLot.NewRow();
                    dtRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOTID"));
                    dtRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID"));
                    dtRow["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "MKT_TYPE_CODE"));
                    dtRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "TRANSFER_QTY")).Equals("") ? 0 : decimal.Parse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "TRANSFER_QTY")));
                    dtRow["CSTID"] = "";

                    dtLot.Rows.Add(dtRow);
                }
                
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_CPROD_CELL_WIP", "INDATA,IN_LOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        ClearControls();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }

                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearControls()
        {
            txtMagLotId.Text = "";
            Util.gridClear(dgTransferMagList);

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSearch())
                    return;

                Search();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Search()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID");
                inTable.Columns.Add("PROCID");
                inTable.Columns.Add("EQSGID");
                inTable.Columns.Add("EQPTID");
                inTable.Columns.Add("PRODUCT_LEVEL2_CODE");
                inTable.Columns.Add("LOTID");

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.STACKING_FOLDING;
                newRow["EQSGID"] = _EqsgId;
                newRow["EQPTID"] = _EqptId;
                newRow["PRODUCT_LEVEL2_CODE"] = "BC";
                newRow["LOTID"] = txtMagLotId.Text;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAITING_MAGAZINE_CPROD", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        if (searchResult.Rows.Count <= 0)
                        {
                            Util.MessageValidation("10009");   //데이터를 찾을 수 없습니다.
                            return;
                        }

                        if (dgTransferMagList.GetRowCount() == 0)
                        {
                            dgTransferMagList.ItemsSource = DataTableConverter.Convert(searchResult);
                        }
                        else
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgTransferMagList.ItemsSource);
                            dtSource.Merge(searchResult);

                            Util.gridClear(dgTransferMagList);
                            dgTransferMagList.ItemsSource = DataTableConverter.Convert(dtSource);
                        }

                        txtMagLotId.SelectAll();
                        txtMagLotId.Focus();
                        txtMagLotId.Text = "";
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
        }

        private void txtMagLotId_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearch_Click(sender, null);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    dgTransferMagList.IsReadOnly = true;

                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
                    DataTable dt = DataTableConverter.Convert(dgTransferMagList.ItemsSource);
                    dt.Rows.RemoveAt(index);

                    dgTransferMagList.ItemsSource = DataTableConverter.Convert(dt);

                    dgTransferMagList.IsReadOnly = false;
                }
            });
        }

        private void txtTransfer_QTY_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            try
            {
                if (sender == null) return;

                C1NumericBox box = (C1NumericBox)sender;

                if (box.Parent == null || ((C1.WPF.DataGrid.DataGridCellPresenter)box.Parent).Row == null) return;

                int i = ((C1.WPF.DataGrid.DataGridCellPresenter)box.Parent).Row.Index;

                if (dgTransferMagList != null && dgTransferMagList.Rows.Count > i)
                {
                    double dWipQty = 0;
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dgTransferMagList.Rows[i].DataItem, "WIPQTY")), out dWipQty);

                    if (dWipQty < e.NewValue)
                    {
                        Util.MessageValidation("SFU4418"); // 입력수량보다 재공수량이 클 수 없습니다.
                        DataTableConverter.SetValue(dgTransferMagList.Rows[i].DataItem, "TRANSFER_QTY", dWipQty);
                    }
                }
            }
            catch (Exception ex)
            {
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
    }
}
