/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.11.18  오화백    : 폴란드, 우크라이나어, 러시아어 숫자형식 관련 수정
  2023.03.15  LEEHJ     : 소형활성화 MES 복사
  2023.07.04  조영대    : FCS002_311_REQUEST_YIELD => FCS001_311_REQUEST_YIELD 복사
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Controls;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_311_REQUEST_YIELD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string reqNo = string.Empty;
        private string reqType = string.Empty;

        DateTime dCalDate;

        public FCS001_311_REQUEST_YIELD()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void InitCombo()
        {

        }

        private void ClearList(object sender, PropertyChangedEventArgs<object> e)
        {
            dgLotList.ClearRows();
            dgCellList.ClearRows();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            CommonCombo _combo = new CommonCombo();
            if (tmps != null && tmps.Length >= 1)
            {
                reqNo = Util.NVC(tmps[0]);
                reqType = Util.NVC(tmps[1]);

                this.Header = ObjectDic.Instance.GetObjectName("전공정 LOSS");
                dgLotList.Columns["REQQTY"].IsReadOnly = false;

                //사유
                string[] sFilter = { "SCRAP_LOT_YIELD" };
                _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "YIELDREASON", sFilter: sFilter);

                dCalDate = GetComSelCalDate();
                dtCalDate.SelectedDateTime = dCalDate;
            }

            if (reqNo.Equals("NEW"))
            {
                btnReqCancel.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (reqType.Equals("LOT_RELEASE"))
                {
                    this.Header = ObjectDic.Instance.GetObjectName("RELEASE요청취소");

                    string[] sFilter = { "UNHOLD_LOT" };
                    _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

                    dgLotList.Columns["REQQTY"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.Header = ObjectDic.Instance.GetObjectName("폐기요청취소");

                    string[] sFilter = { "SCRAP_LOT" };
                    _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);
                }

                btnReq.Visibility = Visibility.Collapsed;
                btnSearch.Visibility = Visibility.Collapsed;
                btnExcel.Visibility = Visibility.Collapsed;
                btnClear.Visibility = Visibility.Collapsed;

                tbCellId.Visibility = Visibility.Collapsed;
                txtCellId.Visibility = Visibility.Collapsed;
                txtGrator.Visibility = Visibility.Collapsed;
                txtNotice.Visibility = Visibility.Collapsed;

                dtCalDate.IsEnabled = false;
                cboResnCode.IsEnabled = false;

                dgCellList.Columns["CHK"].Visibility = Visibility.Collapsed;
                dgCellList.Columns["DELETE_BUTTON"].Visibility = Visibility.Collapsed;

                SetModify();
            }

            btnExcel.Height = 30;
        }

        private void C1Window_Closed(object sender, EventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();

            if (reqNo.Equals("NEW") && reqType.Equals("LOT_SCRAP_YIELD"))  //등록 요청 일 경우만
            {
                txtCellId.Text = string.Empty;
                txtGrator.Text = string.Empty;
                txtNotice.Text = string.Empty;
                txtNote.Text = string.Empty;

                dgCellList.ClearRows();
                dgLotList.ClearRows();

                cboResnCode.SelectedIndex = 0;
            }

            dgGrator.ClearRows();
            dgNotice.ClearRows();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }

        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgLotList.GetRowCount() == 0)
                {
                    Util.Alert("SFU1748");  //요청 목록이 필요합니다.
                    return;
                }

                if (dgGrator.GetRowCount() == 0)
                {
                    Util.Alert("SFU1692");  //승인자가 필요합니다.
                    return;
                }

                if (cboResnCode.GetBindValue() == null) //사유는필수입니다. >> 사유를 선택하세요.
                {
                    cboResnCode.SetValidation("SFU1593");
                    return;
                }

                //요청하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU2924"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Request();
                            }
                        });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReqCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

        private void dtCalDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["DATE"] = dtCalDate.SelectedDateTime.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ERP_CLOSING_FLAG", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                dtCalDate.SelectedDateTime = dCalDate;
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

                    GetCellList(txtCellId.Text);
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

                if (sPasteStrings.Count() > 100)
                {
                    // 최대 100개 까지 가능합니다.
                    Util.MessageValidation("SFU3695", (result) =>
                    {
                        ClearCellId();
                    });
                    return;
                }

                if (!ValidationSearch(true)) return;

                GetCellList(text);

                e.Handled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

            e.Handled = true;
        }

        private void dgCellList_CheckAllChanging(object sender, int row, bool isCheck, RoutedEventArgs e)
        {
            if (dgCellList.Columns["CHK"].Visibility.Equals(Visibility.Visible) && dgCellList.GetValue(row, "NO_CHK").Equals("Y"))
            {
                dgCellList.SetValue(row, "CHK", 0);
            }
        }

        private void dgCellList_CheckAllChanged(object sender, bool isCheck, RoutedEventArgs e)
        {

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

        private void dgCellList_ExecuteCustomBinding(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataSet inSet = e.InData as DataSet;
            string saveCellList = inSet.Tables[0].Rows[0]["SUBLOTID"].Nvc();

            DataSet dsRslt = e.ResultData as DataSet;
            DataTable dtOUTCELL = dsRslt.Tables["OUTCELL"];
            if (dtOUTCELL.Rows.Count > 0)
            {
                DataTable dtCell = null;
                if (dgCellList.ItemsSource == null || dgCellList.GetRowCount() == 0)
                {
                    dtCell = dtOUTCELL.Clone();
                }
                else
                {
                    dtCell = dgCellList.GetDataTable(false);
                }

                for (int i = 0; i < dtOUTCELL.Rows.Count; i++)
                {
                    DataRow dr1 = dtCell.NewRow();
                    if (reqType.Equals("LOT_SCRAP_YIELD"))  //등록 요청 일 경우만
                    {
                        dr1["NO_CHK"] = dtOUTCELL.Rows[i]["NO_CHK"].Nvc();
                        dr1["CHK"] = dr1["NO_CHK"].Equals("Y") ? 0 : 1;
                        //dr1["SMPL_TYPE"] = cboSampleType.GetBindValue();
                        //dr1["SMPL_TYPE_NAME"] = cboSampleType.Text;
                        //dr1["SMPL_MTHD"] = rdoAuto.IsChecked.Equals(true) ? "A" : "M";
                        //dr1["SMPL_MTHD_NAME"] = rdoAuto.IsChecked.Equals(true) ? rdoAuto.Content : rdoManual.Content;
                        //dr1["LOT_TYPE"] = rdoGood.IsChecked.Equals(true) ? "Y" : "N";
                        //dr1["LOT_TYPE_NAME"] = rdoGood.IsChecked.Equals(true) ? rdoGood.Content : rdoNG.Content;
                        dr1["INVALID_CAUSE_MSG"] = dtOUTCELL.Rows[i]["INVALID_CAUSE_MSG"].Nvc();
                        //if ((bool)rdoGood.IsChecked)
                        //{
                        //    dr1["UNPACK_CELL_YN"] = dtOUTCELL.Rows[i]["UNPACK_CELL_YN"].Nvc(); //양품일 경우 포장 대기 cell 구분
                        //}
                        //else
                        //{
                        //    dr1["UNPACK_CELL_YN"] = "F"; // 폐기대기
                        //}
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

                    saveCellList = saveCellList.Replace("," + dr1["SUBLOTID"].Nvc(), "").Replace(dr1["SUBLOTID"].Nvc() + ",", "").Replace(dr1["SUBLOTID"].Nvc(), "");
                }
                dgCellList.SetItemsSource(dtCell, FrameOperation, true);

                DataTable dtOUTLOT = dsRslt.Tables["OUTLOT"];
                if (dtOUTLOT.Rows.Count > 0)
                {
                    DataTable dtLot = null;
                    if (dgLotList.ItemsSource == null || dgLotList.GetRowCount() == 0)
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
                                            validMsg.AppendLine(MessageDic.Instance.GetMessage("SFU2063") + (drLot == null ? "" : " - " + drLot["WIPSNAME"].Nvc()));
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
                            //case "INVALID BIZWF DOC":
                            //    validMsg.AppendLine(MessageDic.Instance.GetMessage("SFU3795"));
                            //    break;
                            //case "INVALID SLOC ID":
                            //    string msg = MessageDic.Instance.GetMessage("FM_ME_0510");
                            //    if (drLot != null)
                            //    {
                            //        msg += " [" + drLot["SLOC_ID"].Nvc() + ", " + drLot["SLOC_ID_CELL"].Nvc() + "]";
                            //    }
                            //    validMsg.AppendLine(msg);
                            //    break;
                            case "INVALID PILOT":
                                validMsg.AppendLine(MessageDic.Instance.GetMessage("SFU8146"));
                                break;
                            default:
                                validMsg.AppendLine(MessageDic.Instance.GetMessage(dr["INVALID_CAUSE_MSG"].Nvc()));
                                break;
                        }
                    }

                    if (validMsg.Length > 0)
                    {
                        dgCellList.SetRowValidation(row, validMsg.Remove(validMsg.Length - 2, 2).ToString());
                    }
                }

                dgCellList.Refresh();
                dgLotList.Refresh();

                if (saveCellList.Length > 3)
                {
                    txtCellId.Text = saveCellList;

                    // Cell 정보가 존재하지않습니다.
                    txtCellId.SetValidation("FM_ME_0021", txtCellId.Text);
                    txtCellId.SelectAll();
                }
                else
                {
                    ClearCellId();
                }
            }
            else
            {
                // Cell 정보가 존재하지않습니다.
                txtCellId.SetValidation("FM_ME_0021", txtCellId.Text);
                txtCellId.SelectAll();
            }

        }

        private void dgCellList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {

        }

        private void txtGrator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (txtGrator.Text.Trim() == string.Empty)
                        return;

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

                        dgGratorSelect.ClearRows();

                        dgGratorSelect.SetItemsSource(dtRslt, FrameOperation, true);

                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void dgGratorChoice_Checked(object sender, RoutedEventArgs e)
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
            drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
            drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
            drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dtTo.Rows.Add(drFrom);
            for (int i = 0; i < dtTo.Rows.Count; i++)
            {
                dtTo.Rows[i]["APPR_SEQS"] = (i + 1);
            }

            dgGrator.SetItemsSource(dtTo, FrameOperation, true);

            dgGratorSelect.Visibility = Visibility.Collapsed;

            txtGrator.Text = "";
        }

        private void txtNotice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {

                    if (txtNotice.Text.Trim() == string.Empty)
                        return;
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

                        dgNotice.ItemsSource = DataTableConverter.Convert(dtTo);

                        txtNotice.Text = "";
                    }
                    else
                    {
                        dgNoticeSelect.Visibility = Visibility.Visible;

                        dgNoticeSelect.ClearRows();
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

        private void dgNoticeChoice_Checked(object sender, RoutedEventArgs e)
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
            drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
            drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
            drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dtTo.Rows.Add(drFrom);


            dgNotice.ItemsSource = DataTableConverter.Convert(dtTo);

            dgNoticeSelect.Visibility = Visibility.Collapsed;

            txtNotice.Text = "";
        }

        private void txtGrator_GotFocus(object sender, RoutedEventArgs e)
        {
            dgGratorSelect.Visibility = Visibility.Collapsed;
        }

        private void txtNotice_GotFocus(object sender, RoutedEventArgs e)
        {
            dgNoticeSelect.Visibility = Visibility.Collapsed;
        }

        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            UcBaseDataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid as UcBaseDataGrid;

            try
            {

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                dg.IsReadOnly = false;
                dg.DeleteRowData(dg.SelectedIndex);
                dg.IsReadOnly = true;

                //승인자 차수 정리
                if (dg.Name.Equals("dgGrator"))
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["APPR_SEQS"] = (i + 1);
                    }

                    dg.ClearRows();
                    dg.SetItemsSource(dt, FrameOperation, true);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCellList_CHK_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod
        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                DateTime dCalDate;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dCalDate = DateTime.Now;

                return dCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        public void GetLotList(string lotLists = "", string CstList = "")
        {
            try
            {
                if (string.IsNullOrEmpty(lotLists) && string.IsNullOrEmpty(CstList) && string.IsNullOrEmpty(txtCellId.Text))
                {
                    Util.MessageValidation("SFU4917"); //LOTID 또는 SKIDID를 입력하세요
                    return;
                }

                const string bizRuleName = "DA_PRD_SEL_HOLD_LOT_LIST";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));

                if (!string.Equals(LoginInfo.CFG_SHOP_ID, "G481") && !string.Equals(LoginInfo.CFG_SHOP_ID, "G482"))
                    inTable.Columns.Add("WIPHOLD", typeof(string));

                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("SKIDID", typeof(string));
                inTable.Columns.Add("PJT", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(lotLists)) dr["LOTID"] = lotLists;

                dr["LOTID"] = txtCellId.GetBindValue();

                if (!string.IsNullOrEmpty(CstList)) dr["SKIDID"] = CstList;

                if (!string.Equals(LoginInfo.CFG_SHOP_ID, "G481") && !string.Equals(LoginInfo.CFG_SHOP_ID, "G482")) dr["WIPHOLD"] = "N";

                dr["WIPSTAT"] = "WAIT";
                inTable.Rows.Add(dr);


                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                if (dtRslt.Rows.Count == 0)
                {
                    if (txtCellId.GetBindValue() == null) //lot id 가 없는 경우
                    {
                        Util.AlertInfo("SFU3537");  //조회된 데이타가 없습니다
                    }
                }
                else
                {
                    dgLotList.ItemsSource = DataTableConverter.Convert(dtRslt);
                }
                txtCellId.Text = "";

                Util.GridSetData(dgLotList, dtRslt, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearCellId()
        {
            txtCellId.Clear();
            txtCellId.Focus();
        }

        private bool ValidationSearch(bool isCopyAndPaste = false)
        {
            txtCellId.ClearValidation();
            //cboSampleType.ClearValidation();

            bool returnValue = true;

            //if (cboSampleType.GetBindValue() == null)
            //{
            //    cboSampleType.SetValidation("SFU4925", tbSampleType.Text);
            //    returnValue = false;
            //    ClearCellId();
            //    return returnValue;
            //}

            if (isCopyAndPaste == false && string.IsNullOrEmpty(txtCellId.Text))
            {
                //조회할 LOT ID 를 입력하세요.
                txtCellId.SetValidation("SFU1190");
                returnValue = false;
                ClearCellId();
            }

            return returnValue;
        }

        private bool ValidationApproval(string approverId)
        {
            const string bizRuleName = "DA_BAS_SEL_AUTHORITYMENU_BY_ID";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("MENUID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["MENUID"] = "SFU010120160";
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

        private void GetCellList(string cellID)
        {
            try
            {
                //cboSampleType.ClearValidation();
                txtCellId.ClearValidation();

                List<string> CellList = null;

                if (reqType.Equals("LOT_SCRAP_YIELD"))  //등록 요청 일 경우만
                {
                    if (string.IsNullOrEmpty(cellID)) return;

                    string[] stringSeparators = new string[] { "," };
                    CellList = cellID.Split(stringSeparators, StringSplitOptions.None).ToList<string>();

                    //스프레드에 있는지 확인
                    bool isDuplicate = false;
                    for (int inx = CellList.Count - 1; inx >= 0; inx--)
                    {
                        for (int i = 0; i < dgCellList.Rows.Count; i++)
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

                DataSet inDataSet = new DataSet();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SUBLOTID", typeof(string));
                inTable.Columns.Add("LOT_TYPE", typeof(string));
                //inTable.Columns.Add("SPLT_FLAG", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOT_REG_TYPE", typeof(string));
                inTable.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                inTable.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //dr["BIZ_WF_REQ_DOC_TYPE_CODE"] = dgBizWFLotHeader.GetValue(0, "BIZ_WF_REQ_DOC_TYPE_CODE").Nvc();
                //dr["BIZ_WF_REQ_DOC_NO"] = dgBizWFLotHeader.GetValue(0, "BIZ_WF_REQ_DOC_NO").Nvc();
                if (reqType.Equals("LOT_SCRAP_YIELD"))
                {
                    dr["LOT_REG_TYPE"] = "REG";

                    dr["SUBLOTID"] = CellList.Aggregate((x, y) => x + "," + y);
                    //dr["LOT_TYPE"] = rdoGood.IsChecked.Equals(true) ? "G" : "N";
                    //dr["SPLT_FLAG"] = "N";
                }
                else
                {
                    dr["LOT_REG_TYPE"] = "CNCL";
                }
                inTable.Rows.Add(dr);
                inDataSet.Tables.Add(inTable);

                // Background 실행
                dgCellList.ExecuteService("BR_FORM_SEL_ERP_BIZWF_ALL_LOSS", "INDATA", "OUTLOT,OUTCELL", inDataSet, argument: inTable, bindTableName: "OUTCELL");

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
                dr["REQ_TYPE"] = reqType;
                dr["REQ_NO"] = reqNo;
                dr["IS_ALL"] = "NONE";
                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_FORM_SEL_APPR_REQUEST", "INDATA", "OUTDATA,OUTPROG,OUTREF,OUTLOT,OUTCELL", inData);

                dgLotList.ClearRows();
                dgLotList.SetItemsSource(dsRslt.Tables["OUTLOT"], FrameOperation);

                dgCellList.ClearRows();
                if (dgCellList.Columns["CHK"].Visibility.Equals(Visibility.Collapsed))
                {
                    dsRslt.Tables["OUTCELL"].AsEnumerable().ToList().ForEach(f => f["CHK"] = 1);
                }
                dgCellList.SetItemsSource(dsRslt.Tables["OUTCELL"], FrameOperation, true);

                dgGrator.ClearRows();
                dgGrator.SetItemsSource(dsRslt.Tables["OUTPROG"], FrameOperation, true);

                dgNotice.ClearRows();
                dgNotice.SetItemsSource(dsRslt.Tables["OUTREF"], FrameOperation, true);

                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    txtNote.Text = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NOTE"].ToString();

                    cboResnCode.SelectedValue = dsRslt.Tables["OUTDATA"].Rows[0]["RESNCODE"].ToString();
                    cboResnCode.IsEditable = false;
                    cboResnCode.IsHitTestVisible = false;
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
                string sTo = "";
                string sCC = "";

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("REQ_NOTE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["IFMODE"] = "OFF";
                row["APPR_BIZ_CODE"] = reqType;
                row["USERID"] = LoginInfo.USERID;
                row["REQ_NOTE"] = txtNote.GetBindValue();
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
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
                    row["LOTID"] = drLot["LOTID"].Nvc();
                    row["LOTID_T"] = drLot["LOTID"].Nvc();
                    row["PRCS_QTY"] = wipQty;
                    row["WIPQTY"] = wipQty;
                    row["WIPQTY2"] = wipQty;
                    //row["ROUTID"] = drLot["ROUTID"].Nvc();
                    //row["PROCID"] = drLot["PROCID"].Nvc();
                    //row["PRODID"] = drLot["BIZ_WF_PRODID"].Nvc();
                    //row["PRODNAME"] = drLot["PRODNAME"].Nvc();
                    //row["MODELID"] = drLot["MODLID"].Nvc();
                    //row["LOTTYPE"] = drLot["LOTTYPE"].Nvc();
                    //row["PROD_LOTID"] = drLot["PROD_LOTID"].Nvc();
                    //row["BIZ_WF_REQ_DOC_TYPE_CODE"] = drLot["BIZ_WF_REQ_DOC_TYPE_CODE"].Nvc();
                    //row["BIZ_WF_REQ_DOC_NO"] = drLot["BIZ_WF_REQ_DOC_NO"].Nvc();
                    //row["BIZ_WF_ITEM_SEQNO"] = drLot["BIZ_WF_ITEM_SEQNO"].NvcDecimal();
                    //if (reqWork.Equals("NEW") && reqType.Equals("REQUEST_CANCEL_BIZWF_LOT"))
                    //{
                    //    row["BIZ_WF_LOT_SEQNO"] = drLot["BIZ_WF_LOT_SEQNO"].NvcDecimal();
                    //}
                    //row["BIZ_WF_SLOC_ID"] = drLot["BIZ_WF_SLOC_ID"].Nvc();
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
                    row["LOTID"] = chkRow["LOTID"].Nvc();
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

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_FORM_REG_APPR_REQUEST", "INDATA,INLOT,INCELL,INPROG,INREF", "OUTDATA,OUTDATA_LOT", inData);
                if (dsRslt != null && dsRslt.Tables.Contains("OUTDATA") &&
                    dsRslt.Tables["OUTDATA"].Rows.Count > 0 && !dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].IsNvc())
                {
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString() + " " + this.Header;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote), inLot));

                    Util.AlertInfo("SFU1747");  //요청되었습니다.
                }
                else
                {
                    Util.AlertInfo("SFU1497");  //데이터 처리 중 오류가 발생했습니다
                }

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ReqCancel()
        {
            string sTo = "";
            string sCC = "";

            //현재상태 체크
            DataTable dtRqstStatus = new DataTable();
            dtRqstStatus.Columns.Add("REQ_NO", typeof(string));

            DataRow drStatus = dtRqstStatus.NewRow();
            drStatus["REQ_NO"] = reqNo;
            dtRqstStatus.Rows.Add(drStatus);

            DataTable dtRsltStatus = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_APPR_REQ", "INDATA", "OUTDATA", dtRqstStatus);

            if (!dtRsltStatus.Rows[0]["REQ_RSLT_CODE"].Equals("REQ"))
            {
                Util.AlertInfo("SFU1691");  //승인이 진행 중입니다.
            }
            else
            {
                //여기까지 현재상태 체크

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("REQ_NO", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["REQ_NO"] = reqNo;
                dr["USERID"] = LoginInfo.USERID;
                dr["REQ_RSLT_CODE"] = "DEL";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_APPR_REQ", "INDATA", "OUTDATA", dtRqst);

                for (int i = 0; i < dgGrator.Rows.Count; i++)
                {
                    if (i == 0)//최초 승인자만 메일 가도록
                    {
                        sTo = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                    }
                }

                //참조자
                for (int i = 0; i < dgNotice.Rows.Count; i++)
                {
                    sCC += Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID")) + ";";
                }


                MailSend mail = new CMM001.Class.MailSend();
                string sMsg = ObjectDic.Instance.GetObjectName("요청취소"); //승인요청취소
                string sTitle = reqNo + " " + this.Header;

                mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, txtNote.Text));

                Util.AlertInfo("SFU1937");  //취소되었습니다.
            }
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        #endregion






    }
}
