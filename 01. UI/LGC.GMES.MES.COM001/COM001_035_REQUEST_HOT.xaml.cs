/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
              DEVELOPER : Initial Created.
  2023.09.06  정재홍    : [E20230802-000826] - 조회 조건 BoxID, EXCEL UPLOAD 버튼 추가
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_REQUEST_HOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;
        private bool _isElectrodProc = false;

        public COM001_035_REQUEST_HOT()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            CommonCombo _combo = new CommonCombo();
            string[] sFilter = { "HOT_LOT" };
            _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);
            if (tmps != null && tmps.Length >= 1)
            {
                _reqNo = Util.NVC(tmps[0]);
                _reqType = Util.NVC(tmps[1]);
            }
            //Lot Release 시 선입선출제외요청 호출(C20180430_75973)
            if (tmps.Length > 2)
            {
                getHotFlag();
                grdSearch.Visibility = Visibility.Collapsed;
                dgListHold.Columns["CHK"].Visibility = Visibility.Collapsed;
                dgRequest.Columns["CHK"].Visibility = Visibility.Collapsed;
                btnReqCancel.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (!_reqNo.Equals("NEW"))
                {
                    SetModify();
                    btnReq.Visibility = Visibility.Collapsed;
                    btnSearchHold.Visibility = Visibility.Collapsed;
                    grdSearch.Visibility = Visibility.Collapsed;
                    txtGrator.Visibility = Visibility.Collapsed;
                    txtNotice.Visibility = Visibility.Collapsed;
                    dgListHold.Columns["CHK"].Visibility = Visibility.Collapsed;
                    dgRequest.Columns["CHK"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnReqCancel.Visibility = Visibility.Collapsed;
                }
            }
            SetAreaByAreaType();    // 공정타입구분을 위하여 추가 [2017-12-29]
        }

        #region [승인자 입력]
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
                        Util.MessageValidation("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {
                        //로그인 사용자와 승인자 동일 사용자 체크
                        if (LoginInfo.USERID == dtRslt.Rows[0]["USERID"].ToString())
                        {
                            Util.MessageValidation("SFU4307");  //로그인 사용자와 승인자가 동일 사용자 입니다.
                            dgGratorSelect.Visibility = Visibility.Collapsed;
                            return;
                        }
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
                            Util.MessageValidation("SFU1779");  //이미 추가 된 승인자 입니다.
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

                        dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

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
        #endregion

        #region [승인자 검색결과 여러개일경우]
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
                Util.MessageValidation("SFU1779");  //이미 추가 된 승인자 입니다.
                dgGratorSelect.Visibility = Visibility.Collapsed;
                return;
            }

            if (!ValidationApproval(DataTableConverter.GetValue(rb.DataContext, "USERID").GetString())) return;

            //로그인 사용자와 승인자 동일 사용자 체크        
            if (Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID")) == LoginInfo.USERID)
            {
                Util.MessageValidation("SFU4307");  //로그인 사용자와 승인자가 동일 사용자 입니다.
                dgGratorSelect.Visibility = Visibility.Collapsed;
                return;
            }

            DataRow drFrom = dtTo.NewRow();
            drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
            drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
            drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dtTo.Rows.Add(drFrom);
            for (int i = 0; i < dtTo.Rows.Count; i++)
            {
                dtTo.Rows[i]["APPR_SEQS"] = (i + 1);
            }

            dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

            dgGratorSelect.Visibility = Visibility.Collapsed;

            txtGrator.Text = "";
        }
        #endregion

        #region [참조자 입력]
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
                    dr["LANGID"] = txtNotice.Text;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU1592");  //사용자 정보가 없습니다.
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
                            Util.MessageValidation("SFU1780."); //이미 추가 된 참조자입니다.
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
        #endregion

        #region [참조자 검색결과 여러개일경우]
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
        #endregion


        private void txtNotice_GotFocus(object sender, RoutedEventArgs e)
        {
            dgGratorSelect.Visibility = Visibility.Collapsed;
        }

        private void txtGrator_GotFocus(object sender, RoutedEventArgs e)
        {
            dgNoticeSelect.Visibility = Visibility.Collapsed;
        }

        #region [제거 처리]
        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;

            try
            {
                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                dg.IsReadOnly = false;
                dg.RemoveRow(dg.SelectedIndex);
                dg.IsReadOnly = true;

                //승인자 차수 정리
                if (dg.Name.Equals("dgGrator"))
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["APPR_SEQS"] = (i + 1);
                    }

                    Util.gridClear(dg);

                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        #endregion

        #region [요청취소]
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        #region [조회클릭]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ( string.IsNullOrEmpty(txtLot.Text.Trim()) && string.IsNullOrEmpty(txtCSTID.Text.Trim()) && string.IsNullOrEmpty(txtPjt.Text.Trim()))
            {
                Util.MessageValidation("SFU4494"); //조회조건 입력 후 조회해야합니다.
                return;
            }

            GetLotList();
            chkAll.IsChecked = false;
        }
        #endregion


        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLot.Text.Trim() == string.Empty)
                        return;
                    GetLotList();
                    chkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [대상 선택하기]
        private void chkHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;

                if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(true) ||
                    DataTableConverter.GetValue(cb.DataContext, "CHK").Nvc().Equals("1") ||
                    DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
                {
                    DataTable dtTo = DataTableConverter.Convert(dgRequest.ItemsSource);

                    if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                    {
                        dtTo.Columns.Add("CHK", typeof(Boolean));
                        dtTo.Columns.Add("EQSGNAME", typeof(string));
                        dtTo.Columns.Add("PROCNAME", typeof(string));
                        dtTo.Columns.Add("LOTID", typeof(string));
                        dtTo.Columns.Add("WIPHOLD", typeof(string));
                        dtTo.Columns.Add("PRODID", typeof(string));
                        dtTo.Columns.Add("PRODNAME", typeof(string));
                        dtTo.Columns.Add("MODELID", typeof(string));
                        dtTo.Columns.Add("WIPQTY", typeof(decimal));
                        dtTo.Columns.Add("WIPQTY2", typeof(decimal));
                        dtTo.Columns.Add("LANE_QTY", typeof(decimal));
                        dtTo.Columns.Add("LANE_PTN_QTY", typeof(decimal));
                        dtTo.Columns.Add("HOT", typeof(string));
                        dtTo.Columns.Add("WH_ID", typeof(string));
                        dtTo.Columns.Add("CSTID", typeof(string));
                        dtTo.Columns.Add("BOXID", typeof(string));
                    }

                    if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'").Length > 0) //중복조건 체크
                    {
                        return;
                    }

                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                    }

                    dtTo.Rows.Add(dr);
                    dgRequest.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else//체크 풀릴때
                {
                    DataTable dtTo = DataTableConverter.Convert(dgRequest.ItemsSource);

                    dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'")[0]);

                    dgRequest.ItemsSource = DataTableConverter.Convert(dtTo);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [요청클릭]
        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //요청하시겠습니까?
                Util.MessageConfirm("SFU2924", (result) =>
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
        #endregion

        #endregion

        #region Mehod

        #region [작업대상 가져오기]
        public void GetLotList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                //dtRqst.Columns.Add("WIPHOLD", typeof(string));
                dtRqst.Columns.Add("SKIDID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("BOXID", typeof(string));

                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_SHOP_ID.Equals("G184"))
                {
                    dtRqst.Columns.Add("WIPSTAT_NOT", typeof(string));
                }

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = "WAIT";
                //dr["WIPHOLD"] = "N";
                //dr["LOTID"] = Util.GetCondition(txtLot, "SFU2839"); //LOTID필수입니다.
                if (!Util.GetCondition(txtLot).Equals(""))
                    dr["LOTID"] = Util.GetCondition(txtLot);
                if (!Util.GetCondition(txtCSTID).Equals(""))
                    dr["SKIDID"] = Util.GetCondition(txtCSTID);
                if (!Util.GetCondition(txtPjt).Equals(""))
                    dr["PJT"] = Util.GetCondition(txtPjt);

                // [E20230802-000826] - speical work UI improvement
                if (!Util.GetCondition(txtBoxid).Equals(""))
                {
                    dr["BOXID"] = Util.GetCondition(txtBoxid);
                    dr["EQSGID"] = LoginInfo.CFG_EQSG_ID; //BOXID 조회시 속도 향상 위해 추가. 기존 조회 조건에 미포함
                }

                if (dr["LOTID"].Equals("") && dr["SKIDID"].Equals(""))
                {
                    Util.MessageInfo("SFU2839");
                    return;
                }

                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_SHOP_ID.Equals("G184"))
                {
                    dr["WIPSTAT_NOT"] = "PROC";
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST", "INDATA", "OUTDATA", dtRqst);

                Util.gridClear(dgListHold);
                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2024");  //해당 LOT은 현재 WAIT 상태가 아니거나 HOLD 상태 입니다.
                }
                else
                {
                    foreach (DataRow row in dtRslt.Rows)
                    {
                        if (_isElectrodProc == true && string.IsNullOrEmpty(Util.NVC(row["WH_ID"])))
                        {
                            Util.MessageValidation("SFU4274", Util.NVC(row["LOTID"]));  //창고에 입고되지않은 LOT[%1]입니다.
                            return;
                        }

                        if (string.Equals(row["HOT"], "Y"))
                        {
                            Util.MessageValidation("SFU4275", Util.NVC(row["LOTID"]));  //해당LOT[% 1]은 이미 선입선출제외요청 되었습니다.
                            return;
                        }
                    }
                    Util.GridSetData(dgListHold, dtRslt, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        #region [수정시 조회]
        public void SetModify()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = _reqNo;

                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_APPR_REQUEST", "INDATA", "OUTDATA,OUTPROG,OUTREF,OUTLOT", inData);

                Util.gridClear(dgRequest);
                dgRequest.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTLOT"]);

                Util.gridClear(dgGrator);
                dgGrator.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTPROG"]);

                Util.gridClear(dgNotice);
                dgNotice.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTREF"]);

                txtNote.Text = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NOTE"].ToString();

                cboResnCode.SelectedValue = dsRslt.Tables["OUTDATA"].Rows[0]["RESNCODE"].ToString();
                cboResnCode.IsEditable = false;
                cboResnCode.IsHitTestVisible = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [LOT Release 선입선출(C20180430_75973)]
        private void getHotFlag()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = _reqNo;

                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_APPR_REQUEST", "INDATA", "OUTDATA,OUTPROG,OUTREF,OUTLOT", inData);

                Util.gridClear(dgRequest);
                dgRequest.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTLOT"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        private void Request()
        {
            string sTo = "";
            string sCC = "";

            if (dgGrator.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1692");  //승인자가 필요합니다.
                return;
            }
            if (dgRequest.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1748");  //요청 목록이 필요합니다.
                return;
            }

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));

            DataRow row = null;
            row = inDataTable.NewRow();
            row["APPR_BIZ_CODE"] = _reqType;
            row["REQ_NOTE"] = Util.GetCondition(txtNote, "SFU1590");    //비고를 입력하세요
            if(cboResnCode.SelectedValue.ToString() != "SELECT") { 
            row["RESNCODE"] = cboResnCode.SelectedValue;
            }

            if (string.IsNullOrEmpty(txtNote.Text))
                return;

            row["USERID"] = LoginInfo.USERID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;

            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("EQSGNAME", typeof(string));
            inLot.Columns.Add("PROCNAME", typeof(string));
            inLot.Columns.Add("PRODID", typeof(string));
            inLot.Columns.Add("PRODNAME", typeof(string));
            inLot.Columns.Add("MODELID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(decimal));
            inLot.Columns.Add("WIPQTY2", typeof(decimal));


            for (int i = 0; i < dgRequest.Rows.Count; i++)
            {
                row = inLot.NewRow();
                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LOTID"));
                row["EQSGNAME"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "EQSGNAME"));
                row["PROCNAME"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PROCNAME"));
                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODID"));
                row["PRODNAME"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODNAME"));
                row["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "MODELID"));
                row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "WIPQTY"));
                row["WIPQTY2"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "WIPQTY2"));
                inLot.Rows.Add(row);
            }

            //승인자
            DataTable inProg = inData.Tables.Add("INPROG");
            inProg.Columns.Add("APPR_SEQS", typeof(string));
            inProg.Columns.Add("APPR_USERID", typeof(string));

            for (int i = 0; i < dgGrator.Rows.Count; i++)
            {
                row = inProg.NewRow();
                row["APPR_SEQS"] = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "APPR_SEQS"));
                row["APPR_USERID"] = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                inProg.Rows.Add(row);

                if (i == 0)//최초 승인자만 메일 가도록
                {
                    sTo = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                }
            }

            //참조자
            DataTable inRef = inData.Tables.Add("INREF");
            inRef.Columns.Add("REF_USERID", typeof(string));

            for (int i = 0; i < dgNotice.Rows.Count; i++)
            {
                row = inRef.NewRow();
                row["REF_USERID"] = Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID"));
                inRef.Rows.Add(row);

                sCC += Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID")) + ";";
            }

            try
            {
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_APPR_REQUEST", "INDATA,INLOT,INPROG,INREF", "OUTDATA,OUTDATA_LOT", inData);
                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    GetLotList();
                    Util.gridClear(dgRequest);
                    Util.gridClear(dgGrator);
                    Util.gridClear(dgNotice);
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString() + " " + this.Header;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, this.makeBodyApp(sTitle, Util.GetCondition(txtNote), inLot));
                }
                Util.MessageInfo("SFU1747");  //요청되었습니다.
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

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("REQ_NO", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["REQ_NO"] = _reqNo;
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
            string sTitle = _reqNo + " " + this.Header;

            mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, this.makeBodyApp(sTitle, Util.GetCondition(txtNote)));

            Util.MessageInfo("SFU1937");  //취소되었습니다.
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private string makeBodyApp(string sTitle, string sContent, DataTable dtLot = null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                sb.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
                sb.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
                sb.Append("<head>");
                sb.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
                sb.Append("<title>Untitled Document</title>");
                sb.Append("<style>");
                sb.Append("	* {margin:0;padding:0;}");
                sb.Append("	body {font-family:Malgun Gothic, Arial, Helvetica, sans-serif;font-size:14px;line-height:1.8;color:#333333;}");
                sb.Append("	table {border-collapse:collapse;width:100%;}");
                sb.Append("	table th {background:#f5f5f5;border-bottom:1px solid #e1e1e1;}");
                sb.Append("	table td {background:#fff;border-right:1px solid #e1e1e1;border-left:1px solid #e1e1e1;border-bottom:1px solid #e1e1e1;}");
                sb.Append("	table tbody th {border-left:1px solid #e1e1e1;text-align:right;padding:6px 8px;		}");
                sb.Append("	table tbody td {text-align:left;padding:6px 8px;}");
                sb.Append("	table thead th {text-align:center;padding:3px;border-right:1px solid #e1e1e1;border-left:1px solid #e1e1e1;	border-bottom:1px solid #d1d1d1;}");
                sb.Append("	.hori-table table tbody td {text-align:center;padding:3px;}");
                sb.Append("	.vertical-table, .hori-table {margin-bottom:20px;}");
                sb.Append("</style>");
                sb.Append("</head>");
                sb.Append("<body>");
                sb.Append("	<div class=\"wrap\">");
                sb.Append("    	<div class=\"vertical-table\">");
                sb.Append("            <table style=\"border-top:2px solid #c8294b; max-width:720px;\">");
                sb.Append("                <tbody>");
                sb.Append("                    <tr>");
                sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("요청구분") + "</th>");
                sb.Append("                        <td>" + sTitle + "</td>");
                sb.Append("                    </tr>");
                sb.Append("                    <tr>");
                sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("사유") + "</th>");
                sb.Append("                        <td>" + sContent.Replace(Environment.NewLine, "<br>") + "&nbsp;</td>");
                sb.Append("                    </tr>                 ");
                sb.Append("                </tbody>");
                sb.Append("            </table>");
                sb.Append("        </div>");
                if (dtLot != null && dtLot.Rows.Count > 0)
                {
                    sb.Append("    <div class=\"hori-table\">");
                    sb.Append("        	<table style=\"border-top:2px solid #c8294b; max-width:720px;\" >");
                    sb.Append("            	<colgroup>");
                    sb.Append("                	<col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                </colgroup>");
                    sb.Append("                <thead>");
                    sb.Append("                	<tr>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("라인") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("LOTID") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("수량") +  "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("제품ID") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("제품명") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("모델ID") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("공정명") + "</th>");
                    sb.Append("                    </tr>");
                    sb.Append("                </thead>");
                    sb.Append("                <tbody>");
                    foreach (DataRow dr in dtLot.Rows)
                    {
                        sb.Append("                	<tr>");
                        sb.Append("                     <td>" + Util.NVC(dr["EQSGNAME"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["LOTID"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC_NUMBER(dr["WIPQTY"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["PRODID"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["PRODNAME"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["MODELID"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["PROCNAME"]) + "&nbsp;</td>");
                        sb.Append("                 </tr>");
                    }
                    sb.Append("                </tbody>");
                    sb.Append("            </table>");
                    sb.Append("        </div>");
                }
                sb.Append("    </div>");
                sb.Append("</body>");
                sb.Append("</html>");


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return sb.ToString();
        }

        //조용수 추가
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
        private void dgListHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try { 
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {

                    if (e.Column.Name.Equals("CHK"))
                    {
 
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                    
                }
                }
                catch(Exception ex)
                {
                    
                }
            }));
    }
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

            chkAllSelect();
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

            chkAllClear();
        }

        private void chkAllSelect()
        {
            Util.gridClear(dgRequest);

            DataTable dtSelect = new DataTable();

            DataTable dtTo = DataTableConverter.Convert(dgListHold.ItemsSource);
            dtSelect = dtTo.Copy();

            dgRequest.ItemsSource = DataTableConverter.Convert(dtSelect);

        }

        private void chkAllClear()
        {
            Util.gridClear(dgRequest);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListHold);
            Util.gridClear(dgRequest);
            txtLot.Text = "";
            txtCSTID.Text = "";
            txtPjt.Text = "";
            chkAll.IsChecked = false;
            cboResnCode.SelectedIndex = 0;

        }

        private void txtCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtCSTID.Text.Trim() == string.Empty)
                        return;
                    GetLotList();
                    chkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetAreaByAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREA_TYPE_CODE"] = Area_Type.ELEC;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_AREA_ID)
                        {
                            _isElectrodProc = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) {}
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
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void txtLotid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
              
                try
                {
                    ShowLoadingIndicator();


                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText() ;
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string sPasteStringLot = "";
                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        sPasteStringLot += sPasteStrings[i]+",";                    
                    }
                    Multi_Create(sPasteStringLot, string.Empty, string.Empty);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        bool Multi_Create(string sLotid, string sCstid, string sBoxid)
        {
            try
            {
                DoEvents();
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                //dtRqst.Columns.Add("WIPHOLD", typeof(string));
                dtRqst.Columns.Add("SKIDID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("BOXID", typeof(string));

                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_SHOP_ID.Equals("G184"))
                {
                    dtRqst.Columns.Add("WIPSTAT_NOT", typeof(string));
                }

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = "WAIT";
                //dr["WIPHOLD"] = "N";

                if (sLotid != string.Empty)
                {
                    dr["LOTID"] = sLotid;
                }
                else if (sCstid != string.Empty)
                {
                    dr["SKIDID"] = sCstid;
                }
                // [E20230802-000826]
                else if (sBoxid != string.Empty)
                {
                    dr["BOXID"] = sBoxid;
                    dr["EQSGID"] = LoginInfo.CFG_EQSG_ID; //BOXID 조회시 속도 향상 위해 추가. 기존 조회 조건에 미포함
                }

                //dr["LOTID"] = Util.GetCondition(txtLot, "SFU2839"); //LOTID필수입니다.

                if (!Util.GetCondition(txtLot).Equals(""))
                    dr["LOTID"] = Util.GetCondition(txtLot);
                if (!Util.GetCondition(txtCSTID).Equals(""))
                    dr["SKIDID"] = Util.GetCondition(txtCSTID);
                if (!Util.GetCondition(txtPjt).Equals(""))
                    dr["PJT"] = Util.GetCondition(txtPjt);

                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_SHOP_ID.Equals("G184"))
                {
                    dr["WIPSTAT_NOT"] = "PROC";
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST", "INDATA", "OUTDATA", dtRqst);

                Util.gridClear(dgListHold);
                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2024");  //해당 LOT은 현재 WAIT 상태가 아니거나 HOLD 상태 입니다.

                }
                else
                {
                    // [E20230802-000826] GetLotList 조회와 Validation 동일하게 적용 [2023.09.18]
                    foreach (DataRow row in dtRslt.Rows)
                    {
                        if (_isElectrodProc == true && string.IsNullOrEmpty(Util.NVC(row["WH_ID"])))
                        {
                            Util.MessageValidation("SFU4274", Util.NVC(row["LOTID"]));  //창고에 입고되지않은 LOT[%1]입니다.
                            return false;
                        }

                        if (string.Equals(row["HOT"], "Y"))
                        {
                            Util.MessageValidation("SFU4275", Util.NVC(row["LOTID"]));  //해당LOT[% 1]은 이미 선입선출제외요청 되었습니다.
                            return false;
                        }
                    }
                    //////////////////////////////////////////////////////////////////////////////////

                    Util.GridSetData(dgListHold, dtRslt, FrameOperation, false);
                }
                return true;
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void txtCSTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {

                try
                {
                    ShowLoadingIndicator();


                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string sPasteStringCst = "";
                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        sPasteStringCst += sPasteStrings[i] + ",";
                    }
                    Multi_Create(string.Empty, sPasteStringCst, string.Empty);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
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

        /// <summary>
        /// E20230802-000826 - speical work UI improvement
        /// </summary>
        private void txtBoxid_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtBoxid.Text.Trim() == string.Empty)
                        return;
                    GetLotList();
                    chkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// E20230802-000826 - speical work UI improvement
        /// </summary>
        private void txtBoxid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string sPasteStringCst = "";
                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        sPasteStringCst += sPasteStrings[i] + ",";
                    }
                    Multi_Create(string.Empty, string.Empty, sPasteStringCst);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }
    }
    #endregion
}
