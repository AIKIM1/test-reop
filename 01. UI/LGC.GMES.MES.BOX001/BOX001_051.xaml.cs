/*************************************************************************************
 Created Date : 2021.01.25
      Creator : INS 김동일K
   Decription : 검사 Skip Pallet 조회 및 승인 처리 (C20201104-000144)
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.25  INS 김동일K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_051.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_051 : UserControl, IWorkArea
    {
        Util _util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;

        string sAREAID_Hist = string.Empty;
        string sSHOPID_Hist = string.Empty;

        public BOX001_051()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            ApplyPermissions();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPalletInfo();
        }

        private void btnApprove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanApprove())
                    return;

                // 승인하시겠습니까?
                Util.MessageConfirm("SFU2878", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ApprProcess();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string sTemp = Util.NVC(cboArea.SelectedValue);
                if (sTemp == "" || sTemp == "SELECT")
                {
                    sAREAID = "";
                    sSHOPID = "";
                }
                else
                {
                    string[] sArry = sTemp.Split('^');
                    sAREAID = sArry[0];
                    sSHOPID = sArry[1];
                }

                String[] sFilter = { sAREAID };    // Area
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboArea_hist_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                string sTemp = Util.NVC(cboArea_hist.SelectedValue);
                if (sTemp == "" || sTemp == "SELECT")
                {
                    sAREAID_Hist = "";
                    sSHOPID_Hist = "";
                }
                else
                {
                    string[] sArry = sTemp.Split('^');
                    sAREAID_Hist = sArry[0];
                    sSHOPID_Hist = sArry[1];
                }

                String[] sFilter = { sAREAID_Hist };    // Area
                _combo.SetCombo(cboEquipmentSegment_hist, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_hist_Click(object sender, RoutedEventArgs e)
        {
            GetQAApprPalletHist();
        }

        private void dgPalletInfo_hist_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgPalletInfo_hist.CurrentRow == null || dgPalletInfo_hist.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left") && Util.NVC(dgPalletInfo_hist.CurrentColumn.Name) == "CHK")
                {
                    string chkValue = Util.NVC(dgPalletInfo_hist.GetCell(dgPalletInfo_hist.CurrentRow.Index, dgPalletInfo_hist.Columns["CHK"].Index).Value);

                    if (chkValue == "0")
                    {
                        DataTableConverter.SetValue(dgPalletInfo_hist.Rows[dgPalletInfo_hist.CurrentRow.Index].DataItem, "CHK", true);

                        for (int idx = 0; idx < dgPalletInfo_hist.Rows.Count; idx++)
                        {
                            if (dgPalletInfo_hist.CurrentRow.Index != idx)
                            {
                                //if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                //    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                //    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                //{
                                //    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                //}
                                DataTableConverter.SetValue(dgPalletInfo_hist.Rows[idx].DataItem, "CHK", false);
                            }
                        }

                        string sPalletid = Util.NVC(dgPalletInfo_hist.GetCell(dgPalletInfo_hist.CurrentRow.Index, dgPalletInfo_hist.Columns["BOXID"].Index).Value);

                        GetCellInfoHist(sPalletid);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgPalletInfo_hist.Rows[dgPalletInfo_hist.CurrentRow.Index].DataItem, "CHK", false);

                        Util.gridClear(dgCell_hist);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //dgPalletInfo_hist.CurrentRow = null;
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnApprove);
            listAuth.Add(btnSearch);
            listAuth.Add(btnSearch_hist);
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            try
            {
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

                _combo.SetCombo(cboArea_hist, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetPalletInfo()
        {
            try
            {
                if (Util.NVC(cboArea.SelectedValue).IndexOf("SELECT") >= 0)
                {
                    Util.MessageValidation("SFU3206"); // 동을 선택해주세요
                    return;
                }
                Util.gridClear(dgCell);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));                
                RQSTDT.Columns.Add("AREAID", typeof(string));                
                RQSTDT.Columns.Add("STDT", typeof(string));
                RQSTDT.Columns.Add("EDDT", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;                
                dr["AREAID"] = sAREAID;
                dr["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue).Equals("") ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["BOXID"] = txtPalletID.Text.Trim() == "" ? null : txtPalletID.Text.Trim();                
                RQSTDT.Rows.Add(dr);
                
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_INSP_SKIP_PLT", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, true);

                        txtNote.Text = "";
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetQAApprPalletHist()
        {
            try
            {
                if (Util.NVC(cboArea_hist.SelectedValue).IndexOf("SELECT") >= 0)
                {
                    Util.MessageValidation("SFU3206"); // 동을 선택해주세요
                    return;
                }
                Util.gridClear(dgCell_hist);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STDT", typeof(string));
                RQSTDT.Columns.Add("EDDT", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sAREAID_Hist;
                dr["STDT"] = dtpDateFrom_hist.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["EDDT"] = dtpDateTo_hist.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment_hist.SelectedValue).Equals("") ? null : Util.NVC(cboEquipmentSegment_hist.SelectedValue);
                dr["BOXID"] = txtPalletID_hist.Text.Trim();
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_INSP_SKIP_PLT_HIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgPalletInfo_hist, dtResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetCellInfo(string sPalletID)
        {
            try
            {
                Util.gridClear(dgCell);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_INSP_SKIP_PLT_CELL_LIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgCell, dtResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetCellInfoHist(string sPalletID)
        {
            try
            {
                Util.gridClear(dgCell_hist);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_INSP_SKIP_PLT_CELL_LIST_HIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgCell_hist, dtResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private bool CanApprove()
        {
            bool bRet = false;

            if (_util.GetDataGridCheckFirstRowIndex(dgPalletInfo, "CHK") < 0)
            {                
                Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                return bRet;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgPalletInfo.Rows[_util.GetDataGridCheckFirstRowIndex(dgPalletInfo, "CHK")].DataItem, "INSP_SKIP_FLAG")).Equals("P"))
            {                
                Util.MessageValidation("SFU5104");  // 이미 승인 요청 진행중인 LOT 입니다.
                return bRet;
            }

            if (string.IsNullOrWhiteSpace(txtNote.Text))
            {
                Util.MessageValidation("SFU1594");  // 사유를 입력하세요.
                return bRet;
            }
            
            bRet = true;
            return bRet;
        }

        private void ApprProcess()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                
                for (int i = 0; i < dgPalletInfo.Rows.Count - dgPalletInfo.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgPalletInfo, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    
                    newRow["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPalletInfo.Rows[i].DataItem, "BOXID"));
                    newRow["NOTE"] = Util.NVC(txtNote.Text);
                    newRow["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                }

                if (inTable.Rows.Count < 1)
                    return;

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("BR_PRD_REG_PALLET_INSP_SKIP_QA_APPR", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        GetPalletInfo();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgPalletInfo_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgPalletInfo.CurrentRow == null || dgPalletInfo.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left") && Util.NVC(dgPalletInfo.CurrentColumn.Name) == "CHK")
                {
                    string chkValue = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["CHK"].Index).Value);

                    if (chkValue == "0")
                    {
                        DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", true);

                        for (int idx = 0; idx < dgPalletInfo.Rows.Count; idx++)
                        {
                            if (dgPalletInfo.CurrentRow.Index != idx)
                            {
                                //if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                //    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                //    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                //{
                                //    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                //}
                                DataTableConverter.SetValue(dgPalletInfo.Rows[idx].DataItem, "CHK", false);
                            }
                        }

                        string sPalletid = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["BOXID"].Index).Value);

                        GetCellInfo(sPalletid);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", false);

                        Util.gridClear(dgCell);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //dgPalletInfo.CurrentRow = null;
            }
        }
        
    }
}
