/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 2023.03.15  LEEHJ     : 소형활성화 MES 복사
 2023.07.04  조영대    : FCS002_311_REQUEST_BIZWFLOT => FCS001_311_REQUEST_BIZWFLOT 복사
 2024.05.28  조영대    : 처리건수를 50건에서 20건으로 줄임.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using C1.WPF.DataGrid;
using System.Text;
using LGC.GMES.MES.CMM001.Controls;
using System.Threading;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_311_REQUEST_BIZWFLOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _reqWork = string.Empty;
        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;
        private int countSearchPerOnce = 20;
        private int countProcessPerOnce = 20;

        public FCS001_311_REQUEST_BIZWFLOT()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                _reqWork = tmps[0].Nvc();
                _reqNo = tmps[1].Nvc();
                _reqType = tmps[2].Nvc();
            }

            // 사용자 설정 제외
            dgCellList.UserConfigExceptColumns.Add("CHK");
            dgCellList.UserConfigExceptColumns.Add("DELETE_BUTTON");

            if (_reqWork.Equals("NEW"))
            {
                if (_reqType.Equals("REQUEST_BIZWF_LOT"))
                {
                    this.Header = ObjectDic.Instance.GetObjectName("BizWF Lot 등록 요청");
                    req_Qty.Visibility = Visibility.Visible;
                    txtCellId.IsEnabled = true;

                    tbSampleType.Visibility = Visibility.Visible;
                    cboSampleType.Visibility = Visibility.Visible;
                    tbSmplMthd.Visibility = Visibility.Visible;
                    rdoAuto.Visibility = Visibility.Visible;
                    rdoManual.Visibility = Visibility.Visible;
                    tbLotType.Visibility = Visibility.Visible;
                    rdoGood.Visibility = Visibility.Visible;
                    rdoNG.Visibility = Visibility.Visible;
                    tbCellId.Visibility = Visibility.Visible;
                    txtCellId.Visibility = Visibility.Visible;
                    btnSearch.Visibility = Visibility.Visible;

                    btnSelectHeader.IsEnabled = true;
                }
                else
                {
                    this.Header = ObjectDic.Instance.GetObjectName("BizWF Lot 등록 취소 요청");
                    btnReq.Content = ObjectDic.Instance.GetObjectName("CANCELATION_REQUEST");
                    req_CancelQty.Visibility = Visibility.Visible;
                    txtCellId.IsEnabled = false;

                    tbSampleType.Visibility = Visibility.Collapsed;
                    cboSampleType.Visibility = Visibility.Collapsed;
                    tbSmplMthd.Visibility = Visibility.Collapsed;
                    rdoAuto.Visibility = Visibility.Collapsed;
                    rdoManual.Visibility = Visibility.Collapsed;
                    tbLotType.Visibility = Visibility.Collapsed;
                    rdoGood.Visibility = Visibility.Collapsed;
                    rdoNG.Visibility = Visibility.Collapsed;
                    tbCellId.Visibility = Visibility.Collapsed;
                    txtCellId.Visibility = Visibility.Collapsed;
                    btnSearch.Visibility = Visibility.Collapsed;

                    btnSelectHeader.IsEnabled = false;

                    dgCellList.Columns["CHK"].Visibility = Visibility.Collapsed;
                    dgCellList.Columns["DELETE_BUTTON"].Visibility = Visibility.Collapsed;

                    SetModify();
                }

                btnClear.IsEnabled = true;

                btnReq.Visibility = Visibility.Visible;
                btnReqCancel.Visibility = Visibility.Collapsed;

                cboSampleType.SetCommonCode("FORM_SMPL_TYPE_CODE", "ATTR1='Y'", CommonCombo.ComboStatus.SELECT, false);
            }
            else
            {
                dgCellList.Columns["CHK"].Visibility = Visibility.Collapsed;
                if (_reqType.Equals("REQUEST_BIZWF_LOT"))
                {
                    this.Header = ObjectDic.Instance.GetObjectName("BizWF Lot 등록 요청 취소");
                    req_Qty.Visibility = Visibility.Visible;
                }
                else
                {
                    this.Header = ObjectDic.Instance.GetObjectName("BizWF Lot 등록 취소 요청 취소");
                    req_CancelQty.Visibility = Visibility.Visible;
                }

                SetModify();

                btnSelectHeader.IsEnabled = false;

                txtCellId.IsEnabled = false;

                btnReq.Visibility = Visibility.Collapsed;
                btnReqCancel.Visibility = Visibility.Visible;

                tbSampleType.Visibility = Visibility.Collapsed;
                cboSampleType.Visibility = Visibility.Collapsed;
                tbSmplMthd.Visibility = Visibility.Collapsed;
                rdoAuto.Visibility = Visibility.Collapsed;
                rdoManual.Visibility = Visibility.Collapsed;
                tbLotType.Visibility = Visibility.Collapsed;
                rdoGood.Visibility = Visibility.Collapsed;
                rdoNG.Visibility = Visibility.Collapsed;
                tbCellId.Visibility = Visibility.Collapsed;
                txtCellId.Visibility = Visibility.Collapsed;
                btnSearch.Visibility = Visibility.Collapsed;

                dgCellList.Columns["DELETE_BUTTON"].Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Event

        private void btnSelectHeader_Click(object sender, RoutedEventArgs e)
        {
            FCS001_311_REQUEST_BIZWFLOT_SEARCH wndPopup = new FCS001_311_REQUEST_BIZWFLOT_SEARCH();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {

                object[] Parameters = new object[1];
                Parameters[0] = _reqType;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupBizWFLotSearch_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndPopupBizWFLotSearch_Closed(object sender, EventArgs e)
        {
            FCS001_311_REQUEST_BIZWFLOT_SEARCH window = sender as FCS001_311_REQUEST_BIZWFLOT_SEARCH;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Clear();

                Util.gridClear(dgBizWFLotHeader);
                Util.gridClear(dgBizWFLotDetail);

                Util.GridSetData(dgBizWFLotHeader, window.BizWFHeader, FrameOperation);
                Util.GridSetData(dgBizWFLotDetail, window.BizWFDetail, FrameOperation);

                dgLotList.ClearRows();
                dgCellList.ClearRows();

                if (_reqType.Equals("REQUEST_BIZWF_LOT") == false)  //등록 취소 일 경우만
                {
                    GetCellList(null, "Cell Add");
                }
            }
        }

        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!ValidationSearch())
                    {
                        return;
                    }

                    GetCellList(txtCellId.Text, "Cell Add");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCellId_ClipboardPasted(object sender, DataObjectPastingEventArgs e, string text)
        {
            try
            {
                string[] stringSeparators = new string[] { "," };
                string[] sPasteStrings = text.Split(stringSeparators, StringSplitOptions.None);

                if (!ValidationSearch(true)) return;

                GetCellList(text, "Cell Add");

                e.Handled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

            e.Handled = true;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
            {
                return;
            }

            GetCellList(txtCellId.Text, "Cell Add");
        }

        private void btnReProcess_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgCellList_CheckAllChanging(object sender, int row, bool isCheck, RoutedEventArgs e)
        {
            if (dgCellList.Columns["CHK"].Visibility.Equals(Visibility.Visible) &&
                dgCellList.GetValue(row, "NO_CHK").Equals("Y"))
            {
                dgCellList.SetValue(row, "CHK", 0);
            }
        }

        private void dgCellList_CheckAllChanged(object sender, bool isCheck, RoutedEventArgs e)
        {
            CalcBizWFDetail();
        }

        private void dgCellList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Presenter == null) return;

            if (e.Cell.Column.Name == "CHK")
            {
                if (dgCellList.GetValue(e.Cell.Row.Index, "NO_CHK").Equals("Y"))
                {
                    e.Cell.Presenter.IsEnabled = false;
                }
                else
                {
                    e.Cell.Presenter.IsEnabled = true;
                }
            }
        }

        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            //BR_PRD_SEL_ERP_BIZWF_LOT 에서 LOT 조회시도 기본 Validation 수행함.
            //Validation 수정시 BR_PRD_SEL_ERP_BIZWF_LOT 도 확인해야 함.

            if (dgBizWFLotHeader.GetRowCount() == 0 || dgBizWFLotDetail.GetRowCount() == 0)
            {
                Util.Alert("SFU3795");  //BizWF 요청서 목록이 없습니다.
                return;
            }

            if (dgCellList.Columns["CHK"].Visibility != Visibility.Collapsed)
            {
                if (!dgCellList.IsCheckedRow("CHK"))
                {
                    Util.Alert("SFU1636");  //선택된 대상이 없습니다.
                    return;
                }
            }
            else
            {
                dgCellList.CheckAll();
            }

            if (dgGrator.GetRowCount() == 0)
            {
                Util.Alert("SFU1692");  //승인자가 필요합니다.
                return;
            }

            // 재점검
            List<string> sublotList = dgCellList.AsEnumerable().Select(s => s.Field<string>("SUBLOTID")).ToList();
            GetCellList(string.Join(",", sublotList), "Request");
        }

        private void btnReqCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgBizWFLotHeader.GetRowCount() == 0 || dgBizWFLotDetail.GetRowCount() == 0)
                {
                    Util.Alert("SFU3795");  //BizWF 요청서 목록이 없습니다.
                    return;
                }

                if (dgGrator.GetRowCount() == 0)
                {
                    Util.Alert("SFU1692");  //승인자가 필요합니다.
                    return;
                }

                if (_reqWork.Equals("MODIFY") && _reqType.Equals("REQUEST_BIZWF_LOT"))
                {
                    if (!dgCellList.IsCheckedRow("CHK"))
                    {
                        Util.Alert("SFU1636");  //선택된 대상이 없습니다.
                        return;
                    }

                }

                //취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                "SFU1243"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ReqCancel();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtGrator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (txtGrator.Text.Trim() == string.Empty)
                    {
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtGrator.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtTo = DataTableConverter.Convert(dgGrator.ItemsSource);

                        if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                        {
                            dtTo.Columns.Add("APPR_SEQS", typeof(string));
                            dtTo.Columns.Add("USERID", typeof(string));
                            dtTo.Columns.Add("USERNAME", typeof(string));
                            dtTo.Columns.Add("DEPTNAME", typeof(string));
                        }

                        if (dtTo.Select("USERID = '" + dtRslt.Rows[0]["USERID"].ToString() + "'").Length > 0) //중복조건 체크
                        {
                            Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                            return;
                        }

                        if (!ValidationApproval(dtRslt.Rows[0]["USERID"].ToString())) return;

                        DataRow drFrom = dtTo.NewRow();
                        foreach (DataColumn dc in dtRslt.Columns)
                        {
                            if (dtTo.Columns.IndexOf(dc.ColumnName) > -1)
                            {
                                drFrom[dc.ColumnName] = dtRslt.Rows[0][dc.ColumnName];
                            }
                        }

                        dtTo.Rows.Add(drFrom);
                        for (int i = 0; i < dtTo.Rows.Count; i++)
                        {
                            dtTo.Rows[i]["APPR_SEQS"] = (i + 1);
                        }

                        dgGrator.SetItemsSource(dtTo, FrameOperation, true);

                        txtGrator.Text = "";
                    }
                    else
                    {
                        dgGratorSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgGratorSelect);

                        dgGratorSelect.ItemsSource = DataTableConverter.Convert(dtRslt);

                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void txtGrator_GotFocus(object sender, RoutedEventArgs e)
        {
            dgGratorSelect.Visibility = Visibility.Collapsed;
        }

        private void txtNotice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {

                    if (txtNotice.Text.Trim() == string.Empty)
                    {
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtNotice.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtTo = DataTableConverter.Convert(dgNotice.ItemsSource);

                        if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                        {
                            dtTo.Columns.Add("USERID", typeof(string));
                            dtTo.Columns.Add("USERNAME", typeof(string));
                            dtTo.Columns.Add("DEPTNAME", typeof(string));
                        }

                        if (dtTo.Select("USERID = '" + dtRslt.Rows[0]["USERID"].ToString() + "'").Length > 0) //중복조건 체크
                        {
                            Util.Alert("SFU1780");  //이미 추가 된 참조자 입니다.
                            return;
                        }

                        DataRow drFrom = dtTo.NewRow();
                        foreach (DataColumn dc in dtRslt.Columns)
                        {
                            if (dtTo.Columns.IndexOf(dc.ColumnName) > -1)
                            {
                                drFrom[dc.ColumnName] = dtRslt.Rows[0][dc.ColumnName];
                            }
                        }

                        dtTo.Rows.Add(drFrom);

                        dgNotice.SetItemsSource(dtTo, FrameOperation, true);

                        txtNotice.Text = "";
                    }
                    else
                    {
                        dgNoticeSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgNoticeSelect);

                        dgNoticeSelect.ItemsSource = DataTableConverter.Convert(dtRslt);

                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void txtNotice_GotFocus(object sender, RoutedEventArgs e)
        {
            dgNoticeSelect.Visibility = Visibility.Collapsed;
        }

        private void chk_Click(object sender, RoutedEventArgs e)
        {
            CalcBizWFDetail();
        }

        private void dgGratorChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                DataTable dtTo = DataTableConverter.Convert(dgGrator.ItemsSource);

                if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                {
                    dtTo.Columns.Add("APPR_SEQS", typeof(string));
                    dtTo.Columns.Add("USERID", typeof(string));
                    dtTo.Columns.Add("USERNAME", typeof(string));
                    dtTo.Columns.Add("DEPTNAME", typeof(string));
                }

                if (dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID").ToString() + "'").Length > 0) //중복조건 체크
                {
                    Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                    dgGratorSelect.Visibility = Visibility.Collapsed;
                    return;
                }

                if (!ValidationApproval(DataTableConverter.GetValue(rb.DataContext, "USERID").GetString())) return;

                DataRow drFrom = dtTo.NewRow();
                drFrom["USERID"] = DataTableConverter.GetValue(rb.DataContext, "USERID").Nvc();
                drFrom["USERNAME"] = DataTableConverter.GetValue(rb.DataContext, "USERNAME").Nvc();
                drFrom["DEPTNAME"] = DataTableConverter.GetValue(rb.DataContext, "DEPTNAME").Nvc();

                dtTo.Rows.Add(drFrom);
                for (int i = 0; i < dtTo.Rows.Count; i++)
                {
                    dtTo.Rows[i]["APPR_SEQS"] = (i + 1);
                }


                dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

                dgGratorSelect.Visibility = Visibility.Collapsed;

                txtGrator.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgNoticeChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                DataTable dtTo = DataTableConverter.Convert(dgNotice.ItemsSource);

                if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                {
                    dtTo.Columns.Add("USERID", typeof(string));
                    dtTo.Columns.Add("USERNAME", typeof(string));
                    dtTo.Columns.Add("DEPTNAME", typeof(string));
                }

                if (dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID").ToString() + "'").Length > 0) //중복조건 체크
                {
                    Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                    dgNoticeSelect.Visibility = Visibility.Collapsed;
                    return;
                }

                DataRow drFrom = dtTo.NewRow();
                drFrom["USERID"] = DataTableConverter.GetValue(rb.DataContext, "USERID").Nvc();
                drFrom["USERNAME"] = DataTableConverter.GetValue(rb.DataContext, "USERNAME").Nvc();
                drFrom["DEPTNAME"] = DataTableConverter.GetValue(rb.DataContext, "DEPTNAME").Nvc();

                dtTo.Rows.Add(drFrom);


                dgNotice.ItemsSource = DataTableConverter.Convert(dtTo);

                dgNoticeSelect.Visibility = Visibility.Collapsed;

                txtNotice.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                C1.WPF.DataGrid.DataGridCellPresenter dgcp = bt.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                if (dgcp.Row == null) return;

                UcBaseDataGrid dg = dgcp.DataGrid as UcBaseDataGrid;

                dg.SelectedIndex = dgcp.Row.Index;

                switch (dg.Name)
                {
                    case "dgCellList":
                        dgCellList.RemoveRowValidation(dg.SelectedIndex);

                        string saveLotId = dg.GetValue(dg.SelectedIndex, "LOTID").Nvc();

                        // Cell 정보 삭제
                        DataTable dtCells = dg.GetDataTable(false);
                        dtCells.Rows[dg.SelectedIndex].Delete();
                        dtCells.AcceptChanges();

                        CalcBizWFDetail();

                        // Lot 정보 삭제
                        if (dtCells.AsEnumerable().Where(w => !w.Field<string>("LOTID").IsNvc() && w.Field<string>("LOTID").Equals(saveLotId)).Count() == 0)
                        {
                            DataTable dtLot = dgLotList.GetDataTable(false);
                            DataRow[] drLot = dtLot.Select("LOTID = '" + saveLotId + "'");
                            foreach (DataRow drDelete in drLot)
                            {
                                drDelete.Delete();
                            }
                            dtLot.AcceptChanges();
                        }

                        if (dgCellList.GetRowCount() > 0)
                        {
                            grdCondition.IsEnabled = false;
                        }
                        else
                        {
                            grdCondition.IsEnabled = true;
                        }
                        break;
                    case "dgNotice":
                        dg.IsReadOnly = false;
                        dg.DeleteRowData(dg.SelectedIndex);
                        dg.IsReadOnly = true;
                        break;
                    case "dgGrator":
                        dg.IsReadOnly = false;
                        dg.DeleteRowData(dg.SelectedIndex);
                        dg.IsReadOnly = true;

                        //승인자 차수 정리
                        DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            dt.Rows[i]["APPR_SEQS"] = (i + 1);
                        }

                        Util.gridClear(dg);

                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private object xProgressCell_WorkProcess(object sender, UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                DataTable inTable = arguments[0] as DataTable;
                List<string> cellList = arguments[1] as List<string>;

                DataSet inDataSet = new DataSet();
                inDataSet.Tables.Add(inTable);

                DataTable dtOUTLOT = null; // 누적 테이블
                DataTable dtOUTCELL = null; // 누적 테이블

                int totalCount = cellList.Count;
                double processCount = Math.Ceiling(totalCount / (double)countSearchPerOnce);

                for (int step = 0; step < processCount; step++)
                {
                    string inCellList = string.Empty;

                    for (int inx = (step * countSearchPerOnce); inx < ((step * countSearchPerOnce) + countSearchPerOnce); inx++)
                    {
                        if (inx >= cellList.Count) break;

                        if (string.IsNullOrEmpty(inCellList))
                        {
                            inCellList = cellList[inx];
                        }
                        else
                        {
                            inCellList = inCellList + "," + cellList[inx];
                        }

                    }

                    inTable.Rows[0]["SUBLOTID"] = inCellList;

                    object[] progressArgument = new object[1] { (step * countSearchPerOnce).Nvc() + " / " + totalCount.Nvc() };

                    int percent = (int)((step * countSearchPerOnce) / (double)totalCount * 100);
                    e.Worker.ReportProgress(percent, progressArgument);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_FORM_SEL_ERP_BIZWF_LOT", "INDATA", "OUTLOT,OUTCELL", inDataSet);
                    if (dtOUTLOT == null)
                    {
                        dtOUTLOT = dsResult.Tables["OUTLOT"].Copy();
                    }
                    else
                    {
                        foreach (DataRow dr in dsResult.Tables["OUTLOT"].Rows)
                        {
                            if (dtOUTLOT.AsEnumerable().Where(w => w["LOTID"].Equals(dr["LOTID"])).Count() == 0)
                            {
                                // MES 2.0 ItemArray 위치 오류 Patch
                                //dtOUTLOT.Rows.Add(dr.ItemArray);
                                dtOUTLOT.AddDataRow(dr);
                            }
                        }
                    }
                    if (dtOUTCELL == null)
                    {
                        dtOUTCELL = dsResult.Tables["OUTCELL"].Copy();
                    }
                    else
                    {
                        dtOUTCELL.Merge(dsResult.Tables["OUTCELL"], true, MissingSchemaAction.Ignore);
                    }
                }

                DataSet dsRslt = new DataSet();
                dsRslt.Tables.Add(dtOUTLOT);
                dsRslt.Tables.Add(dtOUTCELL);

                return dsRslt;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private void xProgressCell_WorkProcessChanged(object sender, int percent, UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] progressArguments = e.Arguments as object[];

                string progressText = progressArguments[0].Nvc();

                xProgressCell.Percent = percent;
                xProgressCell.ProgressText = ObjectDic.Instance.GetObjectName("CHECKING") + " : " + progressText;
                xProgressCell.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void xProgressCell_WorkProcessCompleted(object sender, UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                if (e.Result != null && e.Result is DataSet)
                {
                    List<string> cellList = arguments[1] as List<string>;
                    DataSet dsRslt = e.Result as DataSet;
                    DataTable dtOUTCELL = dsRslt.Tables["OUTCELL"];
                    if (dtOUTCELL.Rows.Count > 0)
                    {
                        DataTable dtCell = null;
                        if (dgCellList.ItemsSource == null || dgCellList.GetRowCount() == 0 || arguments[2].Equals("Request"))
                        {
                            dtCell = dtOUTCELL.Clone();
                        }
                        else
                        {
                            dtCell = dgCellList.GetDataTable(false);
                        }

                        if (!dtCell.Columns.Contains("INVALID_MSG"))
                        {
                            dtCell.Columns.Add("INVALID_MSG");
                        }

                        for (int i = 0; i < dtOUTCELL.Rows.Count; i++)
                        {
                            DataRow dr1 = dtCell.NewRow();
                            if (_reqType.Equals("REQUEST_BIZWF_LOT"))  //등록 요청 일 경우만
                            {
                                dr1["NO_CHK"] = dtOUTCELL.Rows[i]["NO_CHK"].Nvc();
                                dr1["CHK"] = dr1["NO_CHK"].Equals("Y") ? 0 : 1;
                                dr1["SMPL_TYPE"] = cboSampleType.GetBindValue();
                                dr1["SMPL_TYPE_NAME"] = cboSampleType.Text;
                                dr1["SMPL_MTHD"] = rdoAuto.IsChecked.Equals(true) ? "A" : "M";
                                dr1["SMPL_MTHD_NAME"] = rdoAuto.IsChecked.Equals(true) ? rdoAuto.Content : rdoManual.Content;
                                dr1["LOT_TYPE"] = rdoGood.IsChecked.Equals(true) ? "Y" : "N";
                                dr1["LOT_TYPE_NAME"] = rdoGood.IsChecked.Equals(true) ? rdoGood.Content : rdoNG.Content;
                                dr1["INVALID_CAUSE_MSG"] = dtOUTCELL.Rows[i]["INVALID_CAUSE_MSG"].Nvc();
                                if ((bool)rdoGood.IsChecked)
                                {
                                    dr1["UNPACK_CELL_YN"] = dtOUTCELL.Rows[i]["UNPACK_CELL_YN"].Nvc(); //양품일 경우 포장 대기 cell 구분
                                }
                                else
                                {
                                    dr1["UNPACK_CELL_YN"] = "F"; // 폐기대기
                                }
                            }
                            else
                            {
                                dr1["NO_CHK"] = "N";
                                dr1["CHK"] = 1;
                                dr1["SMPL_TYPE"] = dtOUTCELL.Rows[i]["SMPL_TYPE"].Nvc();
                                dr1["SMPL_TYPE_NAME"] = dtOUTCELL.Rows[i]["SMPL_TYPE_NAME"].Nvc();
                                dr1["SMPL_MTHD"] = dtOUTCELL.Rows[i]["SMPL_MTHD"].Nvc();
                                dr1["SMPL_MTHD_NAME"] = dtOUTCELL.Rows[i]["SMPL_MTHD_NAME"].Nvc();
                                dr1["LOT_TYPE"] = dtOUTCELL.Rows[i]["LOT_TYPE"].Nvc();
                                dr1["LOT_TYPE_NAME"] = dtOUTCELL.Rows[i]["LOT_TYPE_NAME"].Nvc();
                                dr1["INVALID_CAUSE_MSG"] = dtOUTCELL.Rows[i]["INVALID_CAUSE_MSG"].Nvc();
                                dr1["UNPACK_CELL_YN"] = dtOUTCELL.Rows[i]["UNPACK_CELL_YN"].Nvc();
                            }

                            dr1["PRODID"] = dtOUTCELL.Rows[i]["PRODID"].Nvc();
                            dr1["SUBLOTID"] = dtOUTCELL.Rows[i]["SUBLOTID"].Nvc();
                            dr1["CSTID"] = dtOUTCELL.Rows[i]["CSTID"].Nvc();
                            dr1["ROUTID"] = dtOUTCELL.Rows[i]["ROUTID"].Nvc();
                            dr1["PROD_LOTID"] = dtOUTCELL.Rows[i]["PROD_LOTID"].Nvc();
                            if (!string.IsNullOrEmpty(dtOUTCELL.Rows[i]["CSTSLOT"].Nvc())) dr1["CSTSLOT"] = dtOUTCELL.Rows[i]["CSTSLOT"].NvcDecimal();
                            dr1["ROUT_NAME"] = dtOUTCELL.Rows[i]["ROUT_NAME"].Nvc();
                            dr1["LOTID"] = dtOUTCELL.Rows[i]["LOTID"].Nvc();
                            dr1["EQSGID"] = dtOUTCELL.Rows[i]["EQSGID"].Nvc();
                            dr1["SUBLOTJUDGE"] = dtOUTCELL.Rows[i]["SUBLOTJUDGE"].Nvc();
                            dr1["LOT_DETL_TYPE_CODE"] = dtOUTCELL.Rows[i]["LOT_DETL_TYPE_CODE"].Nvc();
                            dr1["DFCT_YN"] = dtOUTCELL.Rows[i]["DFCT_YN"].Nvc();
                            dtCell.Rows.Add(dr1);

                            cellList.Remove(dr1["SUBLOTID"].Nvc());
                        }
                        
                        foreach (string cell in cellList)
                        {
                            DataRow dr1 = dtCell.NewRow();
                            dr1["NO_CHK"] = "Y";
                            dr1["CHK"] = 0;
                            dr1["SUBLOTID"] = cell;
                            dr1["INVALID_MSG"] = MessageDic.Instance.GetMessage("FM_ME_0590", cell);
                            dr1["INVALID_CAUSE_MSG"] = "NOT FOUND CELL INFO";
                            dtCell.Rows.Add(dr1);
                        }
                        
                        dgCellList.SetItemsSource(dtCell, FrameOperation, true);

                        DataTable dtOUTLOT = dsRslt.Tables["OUTLOT"];
                        if (dtOUTLOT.Rows.Count > 0)
                        {
                            DataTable dtLot = null;
                            if (dgLotList.ItemsSource == null || dgLotList.GetRowCount() == 0 || arguments[2].Equals("Request"))
                            {
                                dtLot = dtOUTLOT;
                            }
                            else
                            {
                                dtLot = dgLotList.GetDataTable(false);
                                dtLot.Merge(dtOUTLOT, true, MissingSchemaAction.Ignore);
                                dtLot = dtLot.DefaultView.ToTable(true);
                            }
                            dgLotList.SetItemsSource(dtLot, FrameOperation, true);
                        }

                        StringBuilder validMsg = new StringBuilder();
                        for (int row = 0; row < dtCell.Rows.Count; row++)
                        {
                            validMsg.Clear();

                            DataRow dr = dtCell.Rows[row];
                            bool existProdId = false;
                            for (int rowHeader = 0; rowHeader < dgBizWFLotDetail.Rows.Count; rowHeader++)
                            {
                                if (dr["PRODID"].Equals(dgBizWFLotDetail.GetValue(rowHeader, "PRODID"))) existProdId = true;
                            }
                            if (!existProdId && dr["INVALID_MSG"].IsNvc())
                            {
                                dr["CHK"] = 0;
                                dr["NO_CHK"] = "Y";

                                //제품ID가 같지 않습니다.
                                validMsg.AppendLine(MessageDic.Instance.GetMessage("SFU1893"));
                            }

                            if (!dr["INVALID_CAUSE_MSG"].IsNvc())
                            {
                                DataRow drLot = dtOUTLOT.AsEnumerable().Where(w => w.Field<string>("LOTID").Equals(dr["LOTID"])).FirstOrDefault();

                                switch (dr["INVALID_CAUSE_MSG"].Nvc())
                                {
                                    case "INVALID AREA":
                                        // Area 오류
                                        validMsg.AppendLine(MessageDic.Instance.GetMessage("SFU8301"));
                                        break;
                                    case "INVALID PROCID":
                                        // 등록할 수 없는 공정
                                        validMsg.AppendLine(MessageDic.Instance.GetMessage("SFU2063"));
                                        break;
                                    case "INVALID WIPSTAT":
                                        if (drLot != null)
                                        {
                                            switch (drLot["WIPSTAT"].Nvc())
                                            {
                                                case "BIZWF":
                                                    // 요청 중인 상태
                                                    validMsg.AppendLine(MessageDic.Instance.GetMessage("SFU5133"));
                                                    break;
                                                default:
                                                    // 요청할 수없는 상태
                                                    validMsg.AppendLine(MessageDic.Instance.GetMessage("SFU2063") + (drLot == null ? "" : " - " + drLot["WIPSTAT"].Nvc()));
                                                    break;
                                            }
                                        }
                                        break;
                                    case "PACKED STATUS":
                                        validMsg.AppendLine(MessageDic.Instance.GetMessage("SFU8416"));
                                        break;
                                    case "EXIST REQUEST":
                                        validMsg.AppendLine(MessageDic.Instance.GetMessage("FM_ME_0496"));
                                        break;
                                    case "INVALID BIZWF DOC":
                                        if (validMsg.Length == 0)
                                        {
                                            validMsg.AppendLine(MessageDic.Instance.GetMessage("SFU3795"));
                                        }
                                        break;
                                    case "INVALID SLOC ID":
                                        string msg = MessageDic.Instance.GetMessage("FM_ME_0510");
                                        if (drLot != null)
                                        {
                                            msg += " [" + drLot["SLOC_ID"].Nvc() + ", " + drLot["SLOC_ID_CELL"].Nvc() + "]";
                                        }
                                        validMsg.AppendLine(msg);
                                        break;
                                    case "INVALID PILOT":
                                        validMsg.AppendLine(MessageDic.Instance.GetMessage("SFU8146"));
                                        break;
                                    case "NOT FOUND CELL INFO":
                                        validMsg.AppendLine(MessageDic.Instance.GetMessage("FM_ME_0590", dr["SUBLOTID"].Nvc()));
                                        break;
                                    case "WIPQTY 0":
                                        validMsg.AppendLine(MessageDic.Instance.GetMessage("101025"));
                                        break;
                                    case "NOT FOUND PROD LOT":
                                        validMsg.AppendLine(MessageDic.Instance.GetMessage("SFU4014"));
                                        break;
                                    default:
                                        validMsg.AppendLine(MessageDic.Instance.GetMessage(dr["INVALID_CAUSE_MSG"].Nvc()));
                                        break;
                                }
                            }

                            if (validMsg.Length > 0)
                            {
                                string message = (validMsg.ToString() + "[END]").Replace("\r\n[END]", "");
                                if (dgCellList.GetValue(row, "INVALID_MSG").IsNvc())
                                {
                                    dgCellList.SetValue(row, "INVALID_MSG", message);
                                }
                            }
                        }

                        dgCellList.Refresh();
                        dgLotList.Refresh();

                        CalcBizWFDetail();

                        ClearCellId();
                    }
                    else
                    {
                        //정보가 존재하지 않거나, 이미 추출된 Cell 입니다.
                        DataTable dtCell = dgCellList.GetDataTable();
                        if (dtCell == null) dtCell = dtOUTCELL.Clone();

                        foreach (string cell in cellList)
                        {
                            DataRow dr1 = dtCell.NewRow();
                            dr1["NO_CHK"] = "Y";
                            dr1["CHK"] = 0;
                            dr1["SUBLOTID"] = cell;
                            dr1["INVALID_MSG"] = MessageDic.Instance.GetMessage("FM_ME_0590", cell);
                            dr1["INVALID_CAUSE_MSG"] = "NOT FOUND CELL INFO";
                            dtCell.Rows.Add(dr1);
                        }
                        dgCellList.SetItemsSource(dtCell, FrameOperation, true);

                        ClearCellId();
                    }

                    if (dgCellList.GetRowCount() > 0)
                    {
                        grdCondition.IsEnabled = false;
                    }
                    else
                    {
                        grdCondition.IsEnabled = true;
                    }

                    if (arguments[2].Equals("Request"))
                    {
                        for (int inx = 0; inx < dgBizWFLotDetail.Rows.Count; inx++)
                        {
                            int dBizWFLotDetailTotal = dgBizWFLotDetail.GetValue(inx, "TOTAL_WIPQTY").NvcInt();
                            int dBizWFLotDetailReq = dgBizWFLotDetail.GetValue(inx, "BIZ_WF_REQ_QTY").NvcInt();

                            string sBizWFLotDetailProdid = dgBizWFLotDetail.GetValue(inx, "PRODID").Nvc();

                            if (dBizWFLotDetailTotal != dBizWFLotDetailReq)
                            {
                                //요청 수량이 잘못되었습니다.
                                Util.Alert("SFU1749");
                                return;
                            }
                        }

                        //요청하시겠습니까?
                        Util.MessageConfirm("SFU2924", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Request();
                            }
                        });
                    }
                }
                else if (e.Result != null && e.Result is Exception)
                {
                    Util.MessageException(e.Result as Exception);
                }
                else
                {
                    string msg = MessageDic.Instance.GetMessage("FM_ME_0202");
                    Util.AlertInfo(msg);  //작업중 오류가 발생하였습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                xProgressCell.Visibility = Visibility.Collapsed;
                btnClear.IsEnabled = btnReq.IsEnabled = btnReqCancel.IsEnabled = btnSearch.IsEnabled = btnSelectHeader.IsEnabled = true;
            }
        }

        private object xProgress_WorkProcess(object sender, UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                string workType = arguments[0] as string;
                switch (workType)
                {
                    case "추출":
                        {
                            #region 추출 작업
                            DataTable inDataTable = arguments[1] as DataTable;
                            DataTable dtCell = arguments[2] as DataTable;
                            bool isGood = (bool)arguments[3];

                            string BizRuleID = "BR_SET_SMPL_CELL_ALL";
                            if (!isGood) BizRuleID = "BR_SET_NGLOT_SMPL_CELL";

                            DataTable dtInCell = dtCell.Clone();

                            DataSet inDataSet = new DataSet();
                            inDataSet.Tables.Add(inDataTable);
                            inDataSet.Tables.Add(dtInCell);

                            int totalCount = dtCell.Rows.Count;
                            double processCount = Math.Ceiling(totalCount / (double)countProcessPerOnce);

                            for (int step = 0; step < processCount; step++)
                            {
                                dtInCell.Clear();

                                for (int inx = (step * countProcessPerOnce); inx < ((step * countProcessPerOnce) + countProcessPerOnce); inx++)
                                {
                                    if (inx >= dtCell.Rows.Count) break;

                                    // MES 2.0 ItemArray 위치 오류 Patch
                                    //dtInCell.Rows.Add(dtCell.Rows[inx].ItemArray);
                                    dtInCell.AddDataRow(dtCell.Rows[inx]);
                                }

                                object[] progressArgument = new object[1] { (step * countProcessPerOnce).Nvc() + " / " + totalCount.Nvc() };

                                int percent = (int)((step * countProcessPerOnce) / (double)totalCount * 100);
                                e.Worker.ReportProgress(percent, progressArgument);

                                try
                                {
                                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(BizRuleID, "INDATA,INCELL", "OUTDATA", inDataSet);
                                    Thread.Sleep(1000);
                                }
                                catch (Exception ex)
                                {
                                    // 대용량 처리를 위해 오류 발생시 스킵.
                                }
                            }
                            #endregion
                        }
                        return "SUCCESS";
                    case "복구":
                        {
                            #region 복구 작업
                            DataTable inDataTable = arguments[1] as DataTable;
                            DataTable dtCell = arguments[2] as DataTable;

                            string BizRuleID = "BR_SET_SMPL_CELL";

                            DataTable dtInCell = dtCell.Clone();

                            DataSet inDataSet = new DataSet();
                            inDataSet.Tables.Add(inDataTable);
                            inDataSet.Tables.Add(dtInCell);

                            int totalCount = dtCell.Rows.Count;
                            double processCount = Math.Ceiling(totalCount / (double)countProcessPerOnce);

                            for (int step = 0; step < processCount; step++)
                            {
                                dtInCell.Clear();

                                for (int inx = (step * countProcessPerOnce); inx < ((step * countProcessPerOnce) + countProcessPerOnce); inx++)
                                {
                                    if (inx >= dtCell.Rows.Count) break;

                                    // MES 2.0 ItemArray 위치 오류 Patch
                                    //dtInCell.Rows.Add(dtCell.Rows[inx].ItemArray);
                                    dtInCell.AddDataRow(dtCell.Rows[inx]);
                                }

                                object[] progressArgument = new object[1] { (step * countProcessPerOnce).Nvc() + " / " + totalCount.Nvc() };

                                int percent = (int)((step * countProcessPerOnce) / (double)totalCount * 100);
                                e.Worker.ReportProgress(percent, progressArgument);

                                try
                                {
                                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(BizRuleID, "INDATA,INCELL", "OUTDATA", inDataSet);
                                    Thread.Sleep(1000);
                                }
                                catch (Exception ex)
                                {
                                    // 대용량 처리를 위해 오류 발생시 스킵.
                                }
                            }
                            #endregion
                        }
                        return "SUCCESS";
                }
                return "FAIL";
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private void xProgress_WorkProcessChanged(object sender, int percent, UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] progressArguments = e.Arguments as object[];

                string progressText = progressArguments[0].Nvc();

                xProgress.Percent = percent;
                xProgress.ProgressText = progressText;
                xProgress.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void xProgress_WorkProcessCompleted(object sender, UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                string workType = arguments[0] as string;

                if (e.Result != null && e.Result is string)
                {
                    if (e.Result.Nvc().Equals("SUCCESS"))
                    {
                        switch (workType)
                        {
                            case "추출":
                                Util.AlertInfo("SFU1747");  //요청 되었습니다.
                                break;
                            case "복구":
                                Util.AlertInfo("SFU1747");  //요청 되었습니다.
                                break;
                        }

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                    else
                    {
                        Util.AlertInfo("[*]" + e.Result.Nvc());
                    }
                }
                else if (e.Result != null && e.Result is Exception)
                {
                    Util.MessageException(e.Result as Exception);
                }
                else
                {
                    string msg = MessageDic.Instance.GetMessage("FM_ME_0202");
                    Util.AlertInfo(msg);  //작업중 오류가 발생하였습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                xProgress.Visibility = Visibility.Collapsed;
                btnClear.IsEnabled = btnReq.IsEnabled = btnReqCancel.IsEnabled = btnSearch.IsEnabled = btnSelectHeader.IsEnabled = true;
            }
        }

        private void chkViewError_CheckedChanged(object sender, UcBaseCheckBox.CheckedChangedEventArgs e)
        {
            if (e.NewValue)
            {
                DataGridFilterState state = new DataGridFilterState();
                DataGridFilterInfo filterInfo = new DataGridFilterInfo();
                filterInfo.FilterOperation = DataGridFilterOperation.Contains;
                filterInfo.FilterType = DataGridFilterType.Text;
                filterInfo.Value = "Y";
                state.FilterInfo = new List<DataGridFilterInfo>();
                state.FilterInfo.Add(filterInfo);

                dgCellList.FilterBy(dgCellList.Columns["NO_CHK"], state);
            }
            else
            {
                dgCellList.FilterBy(dgCellList.Columns["NO_CHK"], null);
            }
        }
        #endregion

        #region Method

        private void Clear()
        {
            this.ClearValidation();

            if (_reqWork.Equals("NEW") && _reqType.Equals("REQUEST_BIZWF_LOT"))  //등록 요청 일 경우만
            {
                txtCellId.Text = string.Empty;
                txtGrator.Text = string.Empty;
                txtNotice.Text = string.Empty;
                txtNote.Text = string.Empty;

                dgCellList.ClearRows();
                dgLotList.ClearRows();

                CalcBizWFDetail();
            }

            dgGrator.ClearRows();
            dgNotice.ClearRows();

            grdCondition.IsEnabled = true;
        }

        public void SetModify()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_TYPE", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));
                dtRqst.Columns.Add("IS_ALL", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_TYPE"] = _reqType;
                dr["REQ_NO"] = _reqNo;
                dr["IS_ALL"] = "NONE";
                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_FORM_SEL_APPR_REQUEST", "INDATA", "OUTDATA,OUTPROG,OUTREF,OUTLOT,OUTCELL", inData);

                DataTable dtOUTDATA = dsRslt.Tables["OUTDATA"];
                string sBIZ_WF_REQ_DOC_TYPE_CODE = dtOUTDATA.Rows[0]["BIZ_WF_REQ_DOC_TYPE_CODE"].Nvc();
                string sBIZ_WF_REQ_DOC_NO = dtOUTDATA.Rows[0]["BIZ_WF_REQ_DOC_NO"].Nvc();

                SearchBizWFHeader(sBIZ_WF_REQ_DOC_TYPE_CODE, sBIZ_WF_REQ_DOC_NO, "CANCEL");
                SearchBizWFDetail(sBIZ_WF_REQ_DOC_TYPE_CODE, sBIZ_WF_REQ_DOC_NO, "CANCEL");

                dgLotList.ClearRows();
                dgLotList.SetItemsSource(dsRslt.Tables["OUTLOT"], FrameOperation, true);

                dgCellList.ClearRows();
                if (dgCellList.Columns["CHK"].Visibility.Equals(Visibility.Collapsed))
                {
                    dsRslt.Tables["OUTCELL"].AsEnumerable().ToList().ForEach(f => f["CHK"] = 1);
                }
                dgCellList.SetItemsSource(dsRslt.Tables["OUTCELL"], FrameOperation);

                dgGrator.ClearRows();
                dgGrator.SetItemsSource(dsRslt.Tables["OUTPROG"], FrameOperation, true);

                dgNotice.ClearRows();
                dgNotice.SetItemsSource(dsRslt.Tables["OUTREF"], FrameOperation, true);

                txtNote.Text = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NOTE"].Nvc();

                if (dsRslt.Tables["OUTLOT"].Rows[0]["BIZ_WF_LOT_STAT_CODE"].Nvc().Equals("2"))
                {
                    if (dtOUTDATA.Rows[0]["REQ_RSLT_CODE"].Nvc().Equals("REJ"))
                    {
                        tbValidMsg.Text = MessageDic.Instance.GetMessage("SFU1541");

                        btnReq.IsEnabled = false;
                        btnReqCancel.IsEnabled = false;
                        btnClear.IsEnabled = false;
                    }
                    else if (dtOUTDATA.Rows[0]["REQ_RSLT_CODE"].Nvc().Equals("DEL"))
                    {
                        tbValidMsg.Text = MessageDic.Instance.GetMessage("SFU4520");

                        btnReq.IsEnabled = false;
                        btnReqCancel.IsEnabled = false;
                        btnClear.IsEnabled = false;
                    }
                    else if (dtOUTDATA.Rows[0]["REQ_RSLT_CODE"].Nvc().Equals("END") &&
                             _reqType.Equals("REQUEST_BIZWF_LOT"))
                    {
                        tbValidMsg.Text = MessageDic.Instance.GetMessage("SFU5133");

                        btnReq.IsEnabled = false;
                        btnReqCancel.IsEnabled = false;
                        btnClear.IsEnabled = false;
                    }

                }
                else if (dsRslt.Tables["OUTLOT"].Rows[0]["BIZ_WF_LOT_STAT_CODE"].Nvc().Equals("3"))
                {
                    if (dtOUTDATA.Rows[0]["REQ_RSLT_CODE"].Nvc().Equals("END"))
                    {
                        tbValidMsg.Text = MessageDic.Instance.GetMessage("SFU5133");

                        btnReq.IsEnabled = false;
                        btnReqCancel.IsEnabled = false;
                        btnClear.IsEnabled = false;
                    }
                }
                else if (dsRslt.Tables["OUTLOT"].Rows[0]["BIZ_WF_LOT_STAT_CODE"].Nvc().Equals("5"))
                {
                    if (dtOUTDATA.Rows[0]["REQ_RSLT_CODE"].Nvc().Equals("END"))
                    {
                        tbValidMsg.Text = MessageDic.Instance.GetMessage("SFU1690");

                        btnReq.IsEnabled = false;
                        btnReqCancel.IsEnabled = false;
                        btnClear.IsEnabled = false;
                    }
                    else if (dtOUTDATA.Rows[0]["REQ_RSLT_CODE"].Nvc().Equals("REJ"))
                    {
                        tbValidMsg.Text = MessageDic.Instance.GetMessage("SFU1541");

                        btnReq.IsEnabled = false;
                        btnReqCancel.IsEnabled = false;
                        btnClear.IsEnabled = false;
                    }
                }

                CalcBizWFDetail();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationSearch(bool isCopyAndPaste = false)
        {
            txtCellId.ClearValidation();
            //dgCellList.ClearValidation();

            if (dgBizWFLotHeader.GetRowCount() == 0 || dgBizWFLotDetail.GetRowCount() == 0)
            {
                //BizWF 요청서 목록이 없습니다.
                Util.MessageValidation("SFU3795", (result) =>
                {
                    ClearCellId();
                });
                return false;
            }

            bool returnValue = true;

            if (cboSampleType.GetBindValue() == null)
            {
                cboSampleType.SetValidation("SFU4925", tbSampleType.Text);
                returnValue = false;
                ClearCellId();
            }

            if (isCopyAndPaste == false && string.IsNullOrEmpty(txtCellId.Text))
            {
                //조회할 LOT ID 를 입력하세요.
                txtCellId.SetValidation("SFU1190");
                returnValue = false;
                ClearCellId();
            }

            return returnValue;
        }

        private void ClearCellId()
        {
            txtCellId.Clear();
            txtCellId.Focus();
        }

        private void SearchBizWFHeader(string pBIZ_WF_REQ_DOC_TYPE_CODE, string pBIZ_WF_REQ_DOC_NO, string sType)
        {
            try
            {
                Util.gridClear(dgBizWFLotHeader);

                DataTable dtInTable = new DataTable();
                dtInTable.Columns.Add("LANGID", typeof(string));
                dtInTable.Columns.Add("SHOPID", typeof(string));
                dtInTable.Columns.Add("AREAID", typeof(string));
                dtInTable.Columns.Add("REQ_TYPE", typeof(string));
                dtInTable.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                dtInTable.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));
                dtInTable.Columns.Add("IS_COMPLETE", typeof(string));

                DataRow dr = dtInTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["REQ_TYPE"] = sType;
                dr["BIZ_WF_REQ_DOC_TYPE_CODE"] = pBIZ_WF_REQ_DOC_TYPE_CODE;
                dr["BIZ_WF_REQ_DOC_NO"] = pBIZ_WF_REQ_DOC_NO;
                dr["IS_COMPLETE"] = "A";
                dtInTable.Rows.Add(dr);

                const string bizRuleName = "BR_FORM_SEL_ERP_BIZWF_DOC_HEADER";
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtInTable);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU3537");  //조회된 데이타가 없습니다
                }
                else
                {
                    //dgBizWFLotHeader.ItemsSource = DataTableConverter.Convert(dtRslt);
                    Util.GridSetData(dgBizWFLotHeader, dtRslt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchBizWFDetail(string pBIZ_WF_REQ_DOC_TYPE_CODE, string pBIZ_WF_REQ_DOC_NO, string sType)
        {
            try
            {

                Util.gridClear(dgBizWFLotDetail);

                DataTable dtInTable = new DataTable();
                dtInTable.Columns.Add("LANGID", typeof(string));
                dtInTable.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                dtInTable.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));

                DataRow dr = dtInTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BIZ_WF_REQ_DOC_TYPE_CODE"] = pBIZ_WF_REQ_DOC_TYPE_CODE;
                dr["BIZ_WF_REQ_DOC_NO"] = pBIZ_WF_REQ_DOC_NO;

                dtInTable.Rows.Add(dr);

                const string bizRuleName = "DA_SEL_ERP_BIZWF_DOC_DETAIL";
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtInTable);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU3537");  //조회된 데이타가 없습니다
                }
                else
                {
                    Util.GridSetData(dgBizWFLotDetail, dtRslt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Request()
        {
            try
            {
                btnClear.IsEnabled = btnReq.IsEnabled = btnReqCancel.IsEnabled = btnSearch.IsEnabled = btnSelectHeader.IsEnabled = false;

                string sTo = "";
                string sCC = "";

                string BIZ_WF_REQ_DOC_TYPE_CODE = dgBizWFLotHeader.GetValue(0, "BIZ_WF_REQ_DOC_TYPE_CODE").Nvc();
                string BIZ_WF_REQ_DOC_NO = dgBizWFLotHeader.GetValue(0, "BIZ_WF_REQ_DOC_NO").Nvc();

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("REQ_NOTE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("TD_FLAG", typeof(string));
                inDataTable.Columns.Add("SPLT_FLAG", typeof(string));
                inDataTable.Columns.Add("MENUID", typeof(string));
                inDataTable.Columns.Add("USER_IP", typeof(string));
                inDataTable.Columns.Add("PC_NAME", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["IFMODE"] = "OFF";
                row["APPR_BIZ_CODE"] = _reqType;
                row["USERID"] = LoginInfo.USERID;
                row["REQ_NOTE"] = txtNote.GetBindValue();
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["TD_FLAG"] = cboSampleType.GetBindValue();
                row["SPLT_FLAG"] = rdoAuto.IsChecked.Equals(true) ? "A" : "M";
                row["MENUID"] = LoginInfo.CFG_MENUID;
                row["USER_IP"] = LoginInfo.USER_IP;
                row["PC_NAME"] = LoginInfo.PC_NAME;
                row["BIZ_WF_REQ_DOC_TYPE_CODE"] = BIZ_WF_REQ_DOC_TYPE_CODE;
                row["BIZ_WF_REQ_DOC_NO"] = BIZ_WF_REQ_DOC_NO;
                inDataTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = new DataTable("INLOT");
                inLot.Columns.Add("AREAID", typeof(string));
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("LOTID_T", typeof(string));
                inLot.Columns.Add("PRCS_QTY", typeof(decimal));
                inLot.Columns.Add("WIPQTY", typeof(decimal));
                inLot.Columns.Add("WIPQTY2", typeof(decimal));
                inLot.Columns.Add("ROUTID", typeof(string));
                inLot.Columns.Add("PROCID", typeof(string));
                inLot.Columns.Add("PRODID", typeof(string));    //메일 발신용
                inLot.Columns.Add("PRODNAME", typeof(string));  //메일 발신용
                inLot.Columns.Add("MODELID", typeof(string));   //메일 발신용
                inLot.Columns.Add("LOTTYPE", typeof(string));
                inLot.Columns.Add("PROD_LOTID", typeof(string));
                inLot.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                inLot.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));
                inLot.Columns.Add("BIZ_WF_ITEM_SEQNO", typeof(decimal));
                inLot.Columns.Add("BIZ_WF_LOT_SEQNO", typeof(decimal));
                inLot.Columns.Add("BIZ_WF_SLOC_ID", typeof(string));

                foreach (DataRow chkRow in dgCellList.GetCheckedDataRow("CHK"))
                {
                    DataRow drLot = dgLotList.GetDataTable().AsEnumerable().Where(w => w.Field<string>("LOTID").Equals(chkRow["LOTID"])).FirstOrDefault();

                    int wipQty = dgCellList.GetCheckedDataRow("CHK").Where(w => w.Field<string>("LOTID").Equals(drLot["LOTID"])).Count();

                    row = inLot.NewRow();
                    row["AREAID"] = LoginInfo.CFG_AREA_ID;
                    row["LOTID"] = string.Empty;
                    row["LOTID_T"] = drLot["LOTID"].Nvc();
                    row["PRCS_QTY"] = wipQty;
                    row["WIPQTY"] = wipQty;
                    row["WIPQTY2"] = wipQty;
                    row["ROUTID"] = drLot["ROUTID"].Nvc();
                    row["PROCID"] = drLot["PROCID"].Nvc();
                    row["PRODID"] = drLot["BIZ_WF_PRODID"].Nvc();
                    row["PRODNAME"] = drLot["PRODNAME"].Nvc();
                    row["MODELID"] = drLot["MODLID"].Nvc();
                    row["LOTTYPE"] = drLot["LOTTYPE"].Nvc();
                    row["PROD_LOTID"] = drLot["PROD_LOTID"].Nvc();
                    row["BIZ_WF_REQ_DOC_TYPE_CODE"] = drLot["BIZ_WF_REQ_DOC_TYPE_CODE"].Nvc();
                    row["BIZ_WF_REQ_DOC_NO"] = drLot["BIZ_WF_REQ_DOC_NO"].Nvc();
                    row["BIZ_WF_ITEM_SEQNO"] = drLot["BIZ_WF_ITEM_SEQNO"].NvcDecimal();
                    if (_reqWork.Equals("NEW") && _reqType.Equals("REQUEST_CANCEL_BIZWF_LOT"))
                    {
                        row["BIZ_WF_LOT_SEQNO"] = drLot["BIZ_WF_LOT_SEQNO"].NvcDecimal();
                    }
                    row["BIZ_WF_SLOC_ID"] = drLot["BIZ_WF_SLOC_ID"].Nvc();
                    inLot.Rows.Add(row);
                }

                // Distinct 처리
                inLot = inLot.DefaultView.ToTable(true);
                inData.Tables.Add(inLot);

                //대상 CELL
                DataTable inCell = new DataTable("INCELL");
                inCell.Columns.Add("LOTID", typeof(string));
                inCell.Columns.Add("LOTID_T", typeof(string));
                inCell.Columns.Add("SUBLOTID", typeof(string));
                inCell.Columns.Add("SMPL_MTHD", typeof(string));
                inCell.Columns.Add("UNPACK_CELL_YN", typeof(string));
                inCell.Columns.Add("SPLT_FLAG", typeof(string));
                inCell.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                inCell.Columns.Add("LOTTYPE", typeof(string));

                foreach (DataRow chkRow in dgCellList.GetCheckedDataRow("CHK"))
                {
                    DataRow drLot = dgLotList.GetDataTable().AsEnumerable().Where(w => w.Field<string>("LOTID").Equals(chkRow["LOTID"])).FirstOrDefault();

                    row = inCell.NewRow();
                    row["LOTID"] = null; // 가상 LOT 발번
                    row["LOTID_T"] = chkRow["LOTID"].Nvc();
                    row["SUBLOTID"] = chkRow["SUBLOTID"].Nvc();
                    row["SMPL_MTHD"] = chkRow["SMPL_MTHD"].Nvc();
                    row["UNPACK_CELL_YN"] = chkRow["UNPACK_CELL_YN"].Nvc();
                    row["SPLT_FLAG"] = chkRow["SMPL_MTHD"].Nvc().Equals("M") ? "Y" : "N";
                    row["LOT_DETL_TYPE_CODE"] = chkRow["LOT_DETL_TYPE_CODE"].Nvc();
                    inCell.Rows.Add(row);
                }
                inData.Tables.Add(inCell);

                //승인자
                DataTable inProg = inData.Tables.Add("INPROG");
                inProg.Columns.Add("APPR_SEQS", typeof(string));
                inProg.Columns.Add("APPR_USERID", typeof(string));

                for (int i = 0; i < dgGrator.Rows.Count; i++)
                {
                    row = inProg.NewRow();
                    row["APPR_SEQS"] = dgGrator.GetValue(i, "APPR_SEQS").Nvc();
                    row["APPR_USERID"] = dgGrator.GetValue(i, "USERID").Nvc();
                    inProg.Rows.Add(row);

                    if (i == 0)//최초 승인자만 메일 가도록
                    {
                        sTo = dgGrator.GetValue(i, "USERID").Nvc();
                    }
                }

                //참조자
                DataTable inRef = inData.Tables.Add("INREF");
                inRef.Columns.Add("REF_USERID", typeof(string));

                for (int i = 0; i < dgNotice.Rows.Count; i++)
                {
                    row = inRef.NewRow();
                    row["REF_USERID"] = dgNotice.GetValue(i, "USERID").Nvc();
                    inRef.Rows.Add(row);

                    sCC += dgNotice.GetValue(i, "USERID").Nvc() + ";";
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_FORM_REG_APPR_REQUEST", "INDATA,INLOT,INCELL,INPROG,INREF", "OUTDATA,OUTDATA_LOT,OUTDATA_CELL_SPLT_Y,OUTDATA_CELL_SPLT_N", inData);
                if (dsRslt != null && dsRslt.Tables.Contains("OUTDATA") &&
                    dsRslt.Tables["OUTDATA"].Rows.Count > 0 && !dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].IsNvc())
                {
                    #region 승인 요청 메일 발송
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString() + " " + this.Header;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote), inLot));
                    #endregion

                    switch (_reqType)
                    {
                        case "REQUEST_BIZWF_LOT":
                            {
                                #region 개발/기술 Sample Cell 추출 처리

                                DataTable dtProcessCell = new DataTable("INCELL");
                                dtProcessCell.Columns.Add("SUBLOTID", typeof(string));
                                dtProcessCell.Columns.Add("UNPACK_CELL_YN", typeof(string));

                                if (dsRslt.Tables["OUTDATA_CELL_SPLT_Y"] != null && dsRslt.Tables["OUTDATA_CELL_SPLT_Y"].Rows.Count > 0)
                                {
                                    foreach (DataRow drCell in dsRslt.Tables["OUTDATA_CELL_SPLT_Y"].Rows)
                                    {
                                        DataRow newCell = dtProcessCell.NewRow();
                                        newCell["SUBLOTID"] = drCell["SUBLOTID"];
                                        newCell["UNPACK_CELL_YN"] = drCell["UNPACK_CELL_YN"];
                                        dtProcessCell.Rows.Add(newCell);
                                    }
                                }

                                if (dsRslt.Tables["OUTDATA_CELL_SPLT_N"] != null && dsRslt.Tables["OUTDATA_CELL_SPLT_N"].Rows.Count > 0)
                                {
                                    foreach (DataRow drCell in dsRslt.Tables["OUTDATA_CELL_SPLT_N"].Rows)
                                    {
                                        DataRow newCell = dtProcessCell.NewRow();
                                        newCell["SUBLOTID"] = drCell["SUBLOTID"];
                                        newCell["UNPACK_CELL_YN"] = drCell["UNPACK_CELL_YN"];
                                        dtProcessCell.Rows.Add(newCell);
                                    }
                                }

                                if (dtProcessCell.Rows.Count > 0)
                                {
                                    object[] argument = new object[4] { "추출", inDataTable.Copy(), dtProcessCell.Copy(), rdoGood.IsChecked.Equals(true) };

                                    xProgress.Percent = 0;
                                    xProgress.ProgressText = MessageDic.Instance.GetMessage("10057") + " - 0 / " + dtProcessCell.Rows.Count.Nvc();
                                    xProgress.Visibility = Visibility.Visible;

                                    xProgress.RunWorker(argument);
                                }

                                #endregion
                            }
                            break;
                        case "REQUEST_CANCEL_BIZWF_LOT":
                            {
                                #region 개발/기술 Sample Cell 복구 처리

                                DataSet inDataSet = new DataSet();
                                DataTable dtIndata = inDataSet.Tables.Add("INDATA");
                                dtIndata.Columns.Add("USERID", typeof(string));
                                dtIndata.Columns.Add("TD_FLAG", typeof(string));
                                dtIndata.Columns.Add("GLOT_FLAG", typeof(string));

                                DataTable dtInCell = inDataSet.Tables.Add("INCELL");
                                dtInCell.Columns.Add("SUBLOTID", typeof(string));

                                DataRow InRow = dtIndata.NewRow();
                                InRow["USERID"] = LoginInfo.USERID;
                                InRow["TD_FLAG"] = "C";
                                InRow["GLOT_FLAG"] = "Y";
                                dtIndata.Rows.Add(InRow);


                                if (dsRslt.Tables["OUTDATA_CELL_SPLT_Y"] != null && dsRslt.Tables["OUTDATA_CELL_SPLT_Y"].Rows.Count > 0)
                                {
                                    foreach (DataRow drCell in dsRslt.Tables["OUTDATA_CELL_SPLT_Y"].Rows)
                                    {
                                        DataRow newCell = dtInCell.NewRow();
                                        newCell["SUBLOTID"] = drCell["SUBLOTID"];
                                        dtInCell.Rows.Add(newCell);
                                    }
                                }

                                if (dsRslt.Tables["OUTDATA_CELL_SPLT_N"] != null && dsRslt.Tables["OUTDATA_CELL_SPLT_N"].Rows.Count > 0)
                                {
                                    foreach (DataRow drCell in dsRslt.Tables["OUTDATA_CELL_SPLT_N"].Rows)
                                    {
                                        DataRow newCell = dtInCell.NewRow();
                                        newCell["SUBLOTID"] = drCell["SUBLOTID"];
                                        dtInCell.Rows.Add(newCell);
                                    }
                                }

                                if (inDataSet.Tables["INCELL"].Rows.Count > 0)
                                {
                                    object[] argument = new object[3] { "복구", inDataSet.Tables["INDATA"].Copy(), inDataSet.Tables["INCELL"].Copy() };

                                    xProgress.Percent = 0;
                                    xProgress.ProgressText = MessageDic.Instance.GetMessage("10057") + " - 0 / " + inDataSet.Tables["INCELL"].Rows.Count.Nvc();
                                    xProgress.Visibility = Visibility.Visible;

                                    xProgress.RunWorker(argument);
                                }
                                else
                                {
                                    btnClear.IsEnabled = btnReq.IsEnabled = btnReqCancel.IsEnabled = btnSearch.IsEnabled = btnSelectHeader.IsEnabled = true;
                                }
                                #endregion
                            }
                            break;
                    }
                }
                else
                {
                    Util.AlertInfo("SFU1497");  //데이터 처리 중 오류가 발생했습니다
                    btnClear.IsEnabled = btnReq.IsEnabled = btnReqCancel.IsEnabled = btnSearch.IsEnabled = btnSelectHeader.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                btnClear.IsEnabled = btnReq.IsEnabled = btnReqCancel.IsEnabled = btnSearch.IsEnabled = btnSelectHeader.IsEnabled = true;
            }
        }

        private void ReqCancel()
        {
            try
            {
                string sTo = "";
                string sCC = "";
                bool isGood = true;

                //현재상태 체크
                DataTable dtRqstStatus = new DataTable();
                dtRqstStatus.Columns.Add("REQ_NO", typeof(string));

                DataRow drStatus = dtRqstStatus.NewRow();
                drStatus["REQ_NO"] = _reqNo;

                dtRqstStatus.Rows.Add(drStatus);

                DataTable dtRsltStatus = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_APPR_REQ", "INDATA", "OUTDATA", dtRqstStatus);

                if (!dtRsltStatus.Rows[0]["REQ_RSLT_CODE"].Equals("REQ"))
                {
                    Util.AlertInfo("SFU1691");  //승인이 진행 중입니다.
                    return;
                }

                //여기까지 현재상태 체크
                DataSet inDataSet = new DataSet();
                DataTable dtInData = new DataTable("INDATA");
                inDataSet.Tables.Add(dtInData);
                dtInData.Columns.Add("LANGID", typeof(string));
                dtInData.Columns.Add("IFMODE", typeof(string));
                dtInData.Columns.Add("REQ_NO", typeof(string));
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("REQ_RSLT_CODE", typeof(string));
                dtInData.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtInData.Columns.Add("TD_FLAG", typeof(string));
                dtInData.Columns.Add("GLOT_FLAG", typeof(string));

                DataRow drInData = dtInData.NewRow();
                drInData["LANGID"] = LoginInfo.LANGID;
                drInData["IFMODE"] = "OFF";
                drInData["REQ_NO"] = _reqNo;
                drInData["USERID"] = LoginInfo.USERID;
                drInData["REQ_RSLT_CODE"] = "DEL";
                drInData["APPR_BIZ_CODE"] = _reqType;
                drInData["TD_FLAG"] = "C";
                drInData["GLOT_FLAG"] = "Y";
                dtInData.Rows.Add(drInData);

                DataTable dtInCell = new DataTable("INCELL");
                inDataSet.Tables.Add(dtInCell);
                dtInCell.Columns.Add("SUBLOTID", typeof(string));
                dtInCell.Columns.Add("UNPACK_CELL_YN", typeof(string));

                foreach (DataRow chkRow in dgCellList.GetCheckedDataRow("CHK"))
                {
                    DataRow drCell = dtInCell.NewRow();
                    drCell["SUBLOTID"] = chkRow["SUBLOTID"].Nvc();
                    drCell["UNPACK_CELL_YN"] = chkRow["UNPACK_CELL_YN"].Nvc();
                    dtInCell.Rows.Add(drCell);

                    if (_reqType.Equals("REQUEST_CANCEL_BIZWF_LOT"))
                    {
                        drInData["TD_FLAG"] = chkRow["SMPL_TYPE"].Nvc(); ;
                        drInData["GLOT_FLAG"] = null;
                    }

                    isGood = chkRow["LOT_TYPE"].Equals("G");
                }

                for (int i = 0; i < dgGrator.Rows.Count; i++)
                {
                    if (i == 0)//최초 승인자만 메일 가도록
                    {
                        sTo = dgGrator.GetValue(i, "USERID").Nvc();
                    }
                }

                //참조자
                for (int i = 0; i < dgNotice.Rows.Count; i++)
                {
                    sCC += dgNotice.GetValue(i, "USERID").Nvc() + ";";
                }

                if (_reqType.Equals("REQUEST_CANCEL_BIZWF_LOT"))
                {
                    // 취소시 다시 추출
                    try
                    {
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_FORM_REG_APPR_REQUEST_CANCEL", "INDATA,INCELL", "OUTDATA", inDataSet);
                        if (dsRslt != null && dsRslt.Tables.Contains("OUTDATA") &&
                            dsRslt.Tables["OUTDATA"].Rows.Count > 0 && dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].Nvc().Equals("OK"))
                        {
                            #region 승인 요청 메일 발송
                            MailSend mail = new CMM001.Class.MailSend();
                            string sMsg = ObjectDic.Instance.GetObjectName("요청취소"); //승인요청취소
                            string sTitle = _reqNo + " " + this.Header;

                            mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote)));
                            #endregion


                            #region 개발/기술 Sample Cell 추출 처리

                            if (dtInCell.Rows.Count > 0)
                            {
                                object[] argument = new object[4] { "추출", dtInData.Copy(), dtInCell.Copy(), isGood };

                                xProgress.Percent = 0;
                                xProgress.ProgressText = MessageDic.Instance.GetMessage("10057") + " - 0 / " + dtInCell.Rows.Count.Nvc();
                                xProgress.Visibility = Visibility.Visible;

                                btnClear.IsEnabled = btnReq.IsEnabled = btnReqCancel.IsEnabled = btnSearch.IsEnabled = btnSelectHeader.IsEnabled = false;
                                xProgress.RunWorker(argument);
                            }
                            else
                            {
                                this.DialogResult = MessageBoxResult.OK;
                                this.Close();
                            }
                            #endregion
                        }
                        else
                        {
                            //복구실패하였습니다.
                            Util.MessageInfo("FM_ME_0311");
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
                else
                {
                    //기존 Tray로 복구하겠습니까? (버튼을 OK / NO로 생성함)
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0482"), null, "Information", MessageBoxButton.YesNo, MessageBoxIcon.None, (result_restore) =>
                    {
                        if (result_restore == MessageBoxResult.OK)  // 기존TRAY로 복구
                        {
                            drInData["GLOT_FLAG"] = "N";
                        }
                        else if (result_restore == MessageBoxResult.No) // 가상 LOT 발번하여 복구
                        {
                            drInData["GLOT_FLAG"] = "Y";
                        }

                        try
                        {
                            DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_FORM_REG_APPR_REQUEST_CANCEL", "INDATA,INCELL", "OUTDATA", inDataSet);
                            if (dsRslt != null && dsRslt.Tables.Contains("OUTDATA") &&
                                dsRslt.Tables["OUTDATA"].Rows.Count > 0 && dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].Nvc().Equals("OK"))
                            {
                                #region 승인 요청 메일 발송
                                MailSend mail = new CMM001.Class.MailSend();
                                string sMsg = ObjectDic.Instance.GetObjectName("요청취소"); //승인요청취소
                                string sTitle = _reqNo + " " + this.Header;

                                mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote)));
                                #endregion


                                #region 개발/기술 Sample Cell 복구 처리

                                if (inDataSet.Tables["INCELL"].Rows.Count > 0)
                                {
                                    object[] argument = new object[3] { "복구", inDataSet.Tables["INDATA"].Copy(), inDataSet.Tables["INCELL"].Copy() };

                                    xProgress.Percent = 0;
                                    xProgress.ProgressText = MessageDic.Instance.GetMessage("10057") + " - 0 / " + inDataSet.Tables["INCELL"].Rows.Count.Nvc();
                                    xProgress.Visibility = Visibility.Visible;

                                    btnClear.IsEnabled = btnReq.IsEnabled = btnReqCancel.IsEnabled = btnSearch.IsEnabled = btnSelectHeader.IsEnabled = false;
                                    xProgress.RunWorker(argument);
                                }
                                #endregion
                            }
                            else
                            {
                                //복구실패하였습니다.
                                Util.MessageInfo("FM_ME_0311");
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            btnClear.IsEnabled = btnReq.IsEnabled = btnReqCancel.IsEnabled = btnSearch.IsEnabled = btnSelectHeader.IsEnabled = true;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCellList(string cellID, string processType)
        {
            try
            {
                cboSampleType.ClearValidation();
                txtCellId.ClearValidation();

                List<string> CellList = null;

                if (string.IsNullOrEmpty(cellID)) return;

                string[] stringSeparators = new string[] { "," };
                CellList = cellID.Split(stringSeparators, StringSplitOptions.None).ToList<string>();

                if (_reqType.Equals("REQUEST_BIZWF_LOT") && !processType.Equals("Request"))  //등록 요청 일 경우만
                {
                    //스프레드에 있는지 확인
                    bool isDuplicate = false;
                    for (int inx = CellList.Count - 1; inx >= 0; inx--)
                    {
                        for (int i = 0; i < dgCellList.GetRowCount(); i++)
                        {
                            if (dgCellList.GetValue(i, "SUBLOTID").Equals(CellList[inx]))
                            {
                                //목록에 기존재하는 Cell 입니다. 
                                dgCellList.SetRowValidation(i, "FM_ME_0132", dgCellList.GetValue(i, "SUBLOTID").Nvc());
                                isDuplicate = true;
                                CellList.RemoveAt(inx);
                                break;
                            }
                        }
                    }
                    if (isDuplicate && CellList.Count == 0) return;
                }

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SUBLOTID", typeof(string));
                inTable.Columns.Add("LOT_TYPE", typeof(string));
                inTable.Columns.Add("SPLT_FLAG", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOT_REG_TYPE", typeof(string));
                inTable.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                inTable.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["BIZ_WF_REQ_DOC_TYPE_CODE"] = dgBizWFLotHeader.GetValue(0, "BIZ_WF_REQ_DOC_TYPE_CODE").Nvc();
                dr["BIZ_WF_REQ_DOC_NO"] = dgBizWFLotHeader.GetValue(0, "BIZ_WF_REQ_DOC_NO").Nvc();
                if (_reqType.Equals("REQUEST_BIZWF_LOT"))
                {
                    dr["LOT_REG_TYPE"] = "REG";
                    dr["LOT_TYPE"] = rdoGood.IsChecked.Equals(true) ? "G" : "N";
                    dr["SPLT_FLAG"] = "N";
                }
                else
                {
                    dr["LOT_REG_TYPE"] = "CNCL";
                }
                inTable.Rows.Add(dr);

                if (inTable.Rows.Count > 0)
                {
                    object[] argument = new object[3] { inTable, CellList, processType };

                    xProgressCell.Percent = 0;
                    xProgressCell.ProgressText = MessageDic.Instance.GetMessage("10057") + " - 0 / " + CellList.Count.Nvc();
                    xProgressCell.Visibility = Visibility.Visible;

                    btnClear.IsEnabled = btnReq.IsEnabled = btnReqCancel.IsEnabled = btnSearch.IsEnabled = btnSelectHeader.IsEnabled = false;

                    xProgressCell.RunWorker(argument);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CalcBizWFDetail()
        {
            DataTable dtTo = DataTableConverter.Convert(dgCellList.ItemsSource);
            DataTable dtBizwf = DataTableConverter.Convert(dgBizWFLotDetail.ItemsSource);

            if (dtTo.Rows.Count > 0)
            {
                for (int row = dtTo.Rows.Count - 1; row >= 0; row--)
                {
                    if (dtTo.Rows[row]["CHK"].Nvc().Equals("True") ||
                        dtTo.Rows[row]["CHK"].Nvc().Equals("1")) continue;

                    dtTo.Rows[row].Delete();
                }
            }

            for (int idx = 0; idx < dgBizWFLotDetail.Rows.Count; idx++)
            {
                dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = 0;
                dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"] = 0;

                for (int idx_detl = 0; idx_detl < dtTo.Rows.Count; idx_detl++)
                {
                    if (dtTo.Rows[idx_detl]["PRODID"].Equals(dtBizwf.Rows[idx]["PRODID"]))
                    {
                        if (!string.IsNullOrEmpty(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"].ToString()))
                        {
                            dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"] = 1 + Convert.ToDecimal(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"]);
                        }
                        else
                        {
                            dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = 0;
                            dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"] = 1;
                        }
                    }
                }
            }

            if (_reqType.Equals("REQUEST_BIZWF_LOT"))
            {
                for (int index = 0; index < dgBizWFLotDetail.Rows.Count; index++)
                {
                    if (!string.IsNullOrEmpty(dtBizwf.Rows[index]["BIZ_WF_REQ_TOTALQTY"].ToString()))
                    {
                        if (_reqWork.Equals("NEW"))  //신규일때 더하기 그외 빼기
                        {
                            dtBizwf.Rows[index]["TOTAL_WIPQTY"] = Convert.ToDecimal(dtBizwf.Rows[index]["CONFIRM_REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[index]["REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[index]["REQUEST_CANCEL_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[index]["BIZ_WF_REQ_TOTALQTY"]);
                        }
                        else
                        {
                            dtBizwf.Rows[index]["TOTAL_WIPQTY"] = Convert.ToDecimal(dtBizwf.Rows[index]["CONFIRM_REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[index]["REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[index]["REQUEST_CANCEL_WIPQTY"]) - Convert.ToDecimal(dtBizwf.Rows[index]["BIZ_WF_REQ_TOTALQTY"]);
                        }
                    }
                    else
                    {
                        dtBizwf.Rows[index]["TOTAL_WIPQTY"] = Convert.ToDecimal(dtBizwf.Rows[index]["CONFIRM_REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[index]["REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[index]["REQUEST_CANCEL_WIPQTY"]);
                    }
                }
            }
            else
            {
                for (int idx = 0; idx < dgBizWFLotDetail.Rows.Count; idx++)
                {
                    if (!string.IsNullOrEmpty(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"].ToString()))
                    {
                        dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = Convert.ToDecimal(dtBizwf.Rows[idx]["CONFIRM_REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_CANCEL_WIPQTY"]);
                    }
                    else
                    {
                        dtBizwf.Rows[idx]["TOTAL_WIPQTY"] = (Convert.ToDecimal(dtBizwf.Rows[idx]["CONFIRM_REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_WIPQTY"]) + Convert.ToDecimal(dtBizwf.Rows[idx]["REQUEST_CANCEL_WIPQTY"])); // - Convert.ToDecimal(dtBizwf.Rows[idx]["BIZ_WF_REQ_TOTALQTY"]);
                    }

                }
            }

            dgBizWFLotDetail.ItemsSource = DataTableConverter.Convert(dtBizwf);
        }

        private bool ValidationApproval(string approverId)
        {
            const string bizRuleName = "DA_BAS_SEL_AUTHORITYMENU_BY_ID";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("MENUID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["MENUID"] = "SFU010735150";
            dr["USERID"] = approverId;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (dtResult.Rows[0]["ACCESS_COUNT"].GetDecimal() > 0)
                {
                    return true;
                }
                else
                {
                    Util.MessageValidation("SUF4969");  //승인권한이 없는 사용자 입니다.
                    return false;
                }
            }
            else
            {
                Util.MessageValidation("SUF4969");
                return false;
            }
        }

        #endregion
    }
}
