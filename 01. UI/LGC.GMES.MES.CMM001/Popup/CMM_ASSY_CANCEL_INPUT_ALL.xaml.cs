/*************************************************************************************
 Created Date : 2018.06.08
      Creator : INS 김동일K
   Decription : 전지 GMES 고도화 - 조립 공정진척 화면 - 일괄투입취소 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.08  INS 김동일K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
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

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_CANCEL_INPUT_ALL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_CANCEL_INPUT_ALL : C1Window, IWorkArea
    {
        #region Declaration & Constructor        
        private string _ProdLotID = string.Empty;
        private string _ProcID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
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
        public CMM_ASSY_CANCEL_INPUT_ALL()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
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
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 4)
                {
                    _ProcID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _ProdLotID = Util.NVC(tmps[2]);
                    _LDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[3]);
                }
                else
                {
                    _ProcID = "";
                    _EqptID = "";
                    _ProdLotID = "";
                    _LDR_LOT_IDENT_BAS_CODE = "";
                }

                ApplyPermissions();

                GetCurrInList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCancelInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInCancel())
                    return;

                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CancelProcess();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgCurrIn_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //try
            //{
            //    this.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        C1DataGrid dg = sender as C1DataGrid;
            //        if (e.Cell != null &&
            //            e.Cell.Presenter != null &&
            //            e.Cell.Presenter.Content != null)
            //        {
            //            CheckBox chk = e.Cell.Presenter.Content as CheckBox;
            //            if (chk != null)
            //            {
            //                switch (Convert.ToString(e.Cell.Column.Name))
            //                {
            //                    case "CHK":
            //                        if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
            //                           dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
            //                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
            //                           (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
            //                           !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
            //                        {
            //                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
            //                            chk.IsChecked = true;

            //                            for (int idx = 0; idx < dg.Rows.Count; idx++)
            //                            {
            //                                if (e.Cell.Row.Index != idx)
            //                                {
            //                                    if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
            //                                        dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
            //                                        (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
            //                                    {
            //                                        (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
            //                                    }
            //                                    DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
            //                                }
            //                            }
            //                        }
            //                        else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
            //                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
            //                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
            //                                 (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
            //                                 (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
            //                        {
            //                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
            //                            chk.IsChecked = false;
            //                        }
            //                        break;
            //                }

            //                if (dg.CurrentCell != null)
            //                    dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
            //                else if (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null)
            //                    dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

            //            }
            //        }
            //    }));
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void dgLotInfo_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    try
            //    {
            //        if (e == null || e.Column == null || e.Column.Name == null || e.Column.HeaderPresenter == null) return;

            //        if (pre == null) return;

            //        if (string.IsNullOrEmpty(e.Column.Name) == false)
            //        {
            //            if (e.Column.Name.Equals("CHK"))
            //            {
            //                pre.Content = chkAll;
            //                e.Column.HeaderPresenter.Content = pre;
            //                chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
            //                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
            //                chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
            //                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Util.MessageException(ex);
            //    }
            //}));
        }

        private void dgLotInfo_UnloadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //try
                //{
                //    if (e == null || e.Column == null || e.Column.Name == null || e.Column.HeaderPresenter == null) return;

                //    if (pre == null) return;

                //    if (string.IsNullOrEmpty(e.Column.Name) == false)
                //    {
                //        if (e.Column.Name.Equals("CHK"))
                //        {
                //            pre.Content = chkAll;
                //            e.Column.HeaderPresenter.Content = null;
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Util.MessageException(ex);
                //}
            }));
        }

        #endregion

        #region Mehod

        #region [BizCall]

        public void GetCurrInList()
        {
            try
            {
                string sBizNAme = string.Empty;

                //if (_ProcID.Equals(Process.LAMINATION))
                //    sBizNAme = "DA_PRD_SEL_CURR_IN_LOT_LIST_LM";
                //else
                    sBizNAme = "DA_PRD_SEL_CURR_IN_LOT_LIST_FOR_CANCEL_ALL";

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_CURR_IN_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["PROCID"] = PROCID;
                //newRow["EQSGID"] = EQPTSEGMENT;
                newRow["EQPTID"] = _EqptID;
                //newRow["PROD_LOTID"] = PROD_LOTID;
                //newRow["PROD_WIPSEQ"] = PROD_WIPSEQ.Equals("") ? 1 : Convert.ToDecimal(PROD_WIPSEQ);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizNAme, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        
                        Util.GridSetData(dgLotInfo, searchResult, FrameOperation);

                        if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                        {
                            dgLotInfo.Columns["CSTID"].Visibility = Visibility.Visible;
                            dgLotInfo.Columns["CSTID"].IsReadOnly = false;
                            dgLotInfo.Columns["CSTID"].EditOnSelection = true;
                        }
                        else
                        {
                            dgLotInfo.Columns["CSTID"].Visibility = Visibility.Collapsed;
                            dgLotInfo.Columns["CSTID"].IsReadOnly = true;
                            dgLotInfo.Columns["CSTID"].EditOnSelection = false;
                        }

                        // 라미의 경우 컬럼 다르게 보이도록 수정.
                        if (_ProcID.Equals(Process.LAMINATION))
                        {
                            dgLotInfo.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Collapsed;
                            //dgCurrIn.Columns["WIPSNAME"].Visibility = Visibility.Collapsed;
                            dgLotInfo.Columns["MTRLNAME"].Visibility = Visibility.Collapsed;

                            dgLotInfo.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgLotInfo.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Visible;
                            //dgCurrIn.Columns["WIPSNAME"].Visibility = Visibility.Visible;
                            dgLotInfo.Columns["MTRLNAME"].Visibility = Visibility.Visible;

                            dgLotInfo.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Collapsed;
                        }


                        //if (dgLotInfo.CurrentCell != null)
                        //    dgLotInfo.CurrentCell = dgLotInfo.GetCell(dgLotInfo.CurrentCell.Row.Index, dgLotInfo.Columns.Count - 1);
                        //else if (dgLotInfo.Rows.Count > 0 && dgLotInfo.GetCell(dgLotInfo.Rows.Count, dgLotInfo.Columns.Count - 1) != null)
                        //    dgLotInfo.CurrentCell = dgLotInfo.GetCell(dgLotInfo.Rows.Count, dgLotInfo.Columns.Count - 1);
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

        private void CancelProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));

                DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
                inInputTable.Columns.Add("WIPNOTE", typeof(string));
                inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
                inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                //inInputTable.Columns.Add("ACTQTY", typeof(int));
                inInputTable.Columns.Add("INPUT_SEQNO", typeof(Int64));
                inInputTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = _ProcID.Equals(Process.PACKAGING) ? "" : _ProdLotID; // biz에서 찾음.

                inDataTable.Rows.Add(newRow);

                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgLotInfo, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();

                    if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        newRow["WIPNOTE"] = "";
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUT_LOTID"));
                        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        //newRow["INPUT_SEQNO"] = null;
                        newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CSTID"));
                    }
                    else
                    {
                        newRow["WIPNOTE"] = "";
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUT_LOTID"));
                        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUT_QTY")));

                    }
                    inInputTable.Rows.Add(newRow);
                }

                if (inInputTable.Rows.Count < 1) return;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_IN_LOT", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1275");
                        this.DialogResult = MessageBoxResult.OK;
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

        #endregion

        #region [Validation]
        private bool CanCurrInCancel()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
            {
                if (!_Util.GetDataGridCheckValue(dgLotInfo, "CHK", i)) continue;

                if (!_ProcID.Equals(Process.PACKAGING))
                {
                    if (Util.NVC(_ProdLotID).Equals(""))
                    {
                        //Util.Alert("선택된 실적정보가 없습니다.");
                        Util.MessageValidation("SFU1640");
                        return bRet;
                    }
                }

                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                {
                    //Util.Alert("투입 LOT이 없습니다.");
                    Util.MessageValidation("SFU1945");
                    return bRet;
                }

                if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID") && Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CSTID")).Equals(""))
                {
                    Util.MessageValidation("SFU1244");
                    return bRet;
                }
            }

            bRet = true;

            return bRet;
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCancelInput);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

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

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.ItemsSource == null) return;

            DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);
            foreach (DataRow row in dt.Rows)
            {
                //if (Util.NVC(row["DISPATCH_YN"]).Equals("N") && !Util.NVC(row["WIPSTAT"]).Equals("PROC"))
                //{
                    row["CHK"] = true;
                //}
            }
            dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.ItemsSource == null) return;

            DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);
            foreach (DataRow row in dt.Rows)
            {
                row["CHK"] = false;
            }
            dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);
        }

        #endregion

        #endregion
        
    }
}
