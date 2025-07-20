/*************************************************************************************
 Created Date : 2018.07.17
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2018.07.17  DEVELOPER : Initial Created.
  2019-09-09  CHK 버튼 수정
  2023.03.15  LEEHJ  : 소형활성화 MES 복사
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
using DataGridRow = C1.WPF.DataGrid.DataGridRow;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_311_REQUEST_RESERVATION : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _requestNo = string.Empty;
        private string _requestType = string.Empty;

        public FCS002_311_REQUEST_RESERVATION()
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
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent);

            cboArea.SelectedItemChanged += ClearList;
            cboEquipmentSegment.SelectedItemChanged += ClearList;
            cboProcess.SelectedItemChanged += ClearList;
        }

        private void ClearList(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgListHold);
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            CommonCombo combo = new CommonCombo();
            if (tmps != null && tmps.Length >= 1)
            {
                _requestNo = Util.NVC(tmps[0]);
                _requestType = Util.NVC(tmps[1]);
                
                //string[] sFilter1 = { "CHARGE_PROD_LOT" };
                //combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter1);
            }

            if (!_requestNo.Equals("NEW"))
            {
                //string[] sFilter1 = { "CHARGE_PROD_LOT" };
                //combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter1);

                SetModify();
                btnReq.Visibility = Visibility.Collapsed;
                btnSearchHold.Visibility = Visibility.Collapsed;
                btnClear.Visibility = Visibility.Collapsed;
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

        private void txtGrator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (txtGrator.Text.Trim() == string.Empty)
                        return;

                    const string bizRuleName = "DA_BAS_SEL_PERSON_BY_NAME";

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtGrator.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU1592");
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

                        if (dtTo.Select("USERID = '" + dtRslt.Rows[0]["USERID"] + "'").Length > 0) //중복조건 체크
                        {
                            Util.MessageValidation("SFU1779");
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
                            dtTo.Rows[i]["APPR_SEQS"] = i + 1;
                        }

                        dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);
                        txtGrator.Text = string.Empty;
                    }
                    else
                    {
                        dgGratorSelect.Visibility = Visibility.Visible;
                        Util.gridClear(dgGratorSelect);
                        dgGratorSelect.ItemsSource = DataTableConverter.Convert(dtRslt);
                        Focus();
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

            if (rb != null && dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID") + "'").Length > 0) //중복조건 체크
            {
                Util.MessageValidation("SFU1779");
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
                dtTo.Rows[i]["APPR_SEQS"] = i + 1;
            }


            dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);
            dgGratorSelect.Visibility = Visibility.Collapsed;
            txtGrator.Text = string.Empty;
        }


        private void txtNotice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (txtNotice.Text.Trim() == string.Empty)
                        return;
                    const string bizRuleName = "DA_BAS_SEL_PERSON_BY_NAME";

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtNotice.Text;
                    dr["LANGID"] = txtNotice.Text;
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU1592");
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

                        if (dtTo.Select("USERID = '" + dtRslt.Rows[0]["USERID"] + "'").Length > 0) //중복조건 체크
                        {
                            Util.Alert("이미추가된참조자입니다.");
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
                        txtNotice.Text = string.Empty;
                    }
                    else
                    {
                        dgNoticeSelect.Visibility = Visibility.Visible;
                        Util.gridClear(dgNoticeSelect);
                        dgNoticeSelect.ItemsSource = DataTableConverter.Convert(dtRslt);
                        Focus();
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

            if (rb != null && dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID") + "'").Length > 0) //중복조건 체크
            {
                Util.MessageValidation("SFU1779");
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
            txtNotice.Text = string.Empty;
        }


        private void txtNotice_GotFocus(object sender, RoutedEventArgs e)
        {
            dgGratorSelect.Visibility = Visibility.Collapsed;
        }

        private void txtGrator_GotFocus(object sender, RoutedEventArgs e)
        {
            dgNoticeSelect.Visibility = Visibility.Collapsed;
        }

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
                        dt.Rows[i]["APPR_SEQS"] = i + 1;
                    }

                    Util.gridClear(dg);
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
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
                Util.MessageConfirm("SFU1243", (result) =>
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
            _chkAll.IsChecked = false;
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
                    _chkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLot.Text.Trim() == string.Empty)
                        return;
                    GetLotList();
                    _chkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void txtLot_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    try
                    {
                        ShowLoadingIndicator();

                        string[] stringSeparators = new string[] { "\r\n" };
                        string sPasteString = Clipboard.GetText();
                        string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                        string sPasteStringLot = "";
                        if (sPasteStrings.Count() > 100)
                        {
                            Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                            return;
                        }

                        if (sPasteStrings.Count() == 1)
                        {
                            GetLotList(sPasteStrings[0]);
                        } else
                        {
                            foreach (string item in sPasteStrings)
                            {
                                sPasteStringLot += item + ",";
                            }

                            GetMultiLotList(sPasteStringLot, string.Empty);
                        }
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

                if (DataTableConverter.GetValue(cb.DataContext, "REL_SYSTEM_ID").Equals("OCAP"))
                {
                    Util.MessageValidation("SUF9013", DataTableConverter.GetValue(cb.DataContext, "LOTID").ToString());   //OCAP 처리 대상은 요청 목록에서 제외됩니다. (LOT ID : %1)

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Util.NVC(row["REL_SYSTEM_ID"]).Equals("OCAP"))
                        {
                            row["CHK"] = false;
                        }
                    }

                    return;
                }


                if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
                {
                    DataTable dtTo = DataTableConverter.Convert(dgRequest.ItemsSource);

                    if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                    {
                        dtTo.Columns.Add("CHK", typeof(bool));
                        dtTo.Columns.Add("EQSGNAME", typeof(string));
                        dtTo.Columns.Add("LOTID", typeof(string));
                        dtTo.Columns.Add("PRODID", typeof(string));
                        dtTo.Columns.Add("PRODNAME", typeof(string));
                        dtTo.Columns.Add("MODELID", typeof(string));
                        dtTo.Columns.Add("WIPQTY", typeof(decimal));
                        dtTo.Columns.Add("REQQTY", typeof(decimal));
                        dtTo.Columns.Add("WIPQTY2", typeof(decimal));
                        dtTo.Columns.Add("LANE_QTY", typeof(decimal));
                        dtTo.Columns.Add("LANE_PTN_QTY", typeof(decimal));
                        dtTo.Columns.Add("REL_SYSTEM_ID", typeof(string));
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

        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //요청하시겠습니까?
                Util.MessageConfirm("SFU2924", result =>
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

        #region Mehod

        private void GetLotList(string sLotid = null)
        {
            try
            {
                if (!ValidationGetLot(sLotid)) return;
                //if (Util.GetCondition(txtLot).Equals("") && Util.GetCondition(txtCSTID).Equals("") && string.IsNullOrEmpty(sLotid))
                //    return;

                //const string bizRuleName = "DA_PRD_SEL_HOLD_LOT_LIST";
                const string bizRuleName = "DA_PRD_SEL_HOLD_LOT_LIST_SINGLE_LOT";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("WIPHOLD", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("SKIDID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = "WAIT"; //-WAIT : 공정 대기
                dr["WIPHOLD"] = "N"; //-현재 HOLD 아닌것들

                if (Util.GetCondition(txtLot).Equals("") && Util.GetCondition(txtCSTID).Equals("") && string.IsNullOrEmpty(sLotid))
                {//LOT 이나 SKID 가 입력되지 않았을 경우

                    //if (Util.GetCondition(txtProd).Equals("") && Util.GetCondition(txtModl).Equals("") && Util.GetCondition(txtPjt).Equals(""))
                    //{
                    //    return;
                    //}

                    dr["AREAID"] = Util.GetCondition(cboArea);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                    dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);

                    if (!Util.GetCondition(txtProd).Equals(""))
                    {
                        dr["PRODID"] = Util.GetCondition(txtProd);
                    }

                    if (!Util.GetCondition(txtModl).Equals(""))
                    {
                        dr["MODLID"] = Util.GetCondition(txtModl);
                    }

                    if (!Util.GetCondition(txtPjt).Equals(""))
                    {
                        dr["PJT"] = Util.GetCondition(txtPjt);
                    }
                }
                else //LOT 이나 SKID 가 입력되었을 경우
                {
                    if (Util.GetCondition(txtLot).Equals("") && string.IsNullOrEmpty(sLotid))
                    {
                        if (!Util.GetCondition(txtCSTID).Equals(""))
                        {
                            dr["SKIDID"] = Util.GetCondition(txtCSTID);
                        }
                    }
                    else
                    {
                        if (Util.GetCondition(txtLot).Equals(""))
                        {
                            dr["LOTID"] = sLotid;
                        }
                        else
                        {
                            dr["LOTID"] = Util.GetCondition(txtLot);
                        }
                    }
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

                Util.gridClear(dgListHold);
                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2024");
                }
                else
                {
                    dgListHold.ItemsSource = DataTableConverter.Convert(dtRslt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationGetLot(string sLotid)
        {
            if (!string.IsNullOrEmpty(sLotid) && sLotid.Length < 7)
            {
                Util.MessageValidation("SFU4074", "7"); //LOTID %1자리 이상 입력시 조회 가능합니다.
                return false;
            }

            if (!Util.GetCondition(txtLot).Equals("") && txtLot.Text.Length < 7)
            {
                Util.MessageValidation("SFU4074", "7"); //LOTID %1자리 이상 입력시 조회 가능합니다.
                return false;
            }

            if (Util.GetCondition(txtLot).Equals("") && Util.GetCondition(txtCSTID).Equals("") && string.IsNullOrEmpty(sLotid))
            {
                if (Util.GetCondition(cboArea).Equals(""))
                {
                    Util.MessageValidation("SFU3206"); //동을 선택해주세요
                    return false;
                }
            }

            return true;
        }

        private void GetMultiLotList(string sLotid, string sCstid)
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
                dtRqst.Columns.Add("SKIDID", typeof(string));
                //dtRqst.Columns.Add("PJT", typeof(string));


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
                //dr["LOTID"] = Util.GetCondition(txtLot, "SFU2839"); //LOTID필수입니다.

                if (!Util.GetCondition(txtLot).Equals(""))
                    dr["LOTID"] = Util.GetCondition(txtLot);
                if (!Util.GetCondition(txtCSTID).Equals(""))
                    dr["SKIDID"] = Util.GetCondition(txtCSTID);
                //if (!Util.GetCondition(txtPjt).Equals(""))
                //    dr["PJT"] = Util.GetCondition(txtPjt);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST", "INDATA", "OUTDATA", dtRqst);

                Util.gridClear(dgListHold);
                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2024");  //해당 LOT은 현재 WAIT 상태가 아니거나 HOLD 상태 입니다.
                }
                else
                {
                    Util.GridSetData(dgListHold, dtRslt, FrameOperation, false);
                }
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
                const string bizRuleName = "BR_PRD_SEL_APPR_REQUEST";

                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = _requestNo;

                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTDATA,OUTPROG,OUTREF,OUTLOT", inData);

                Util.gridClear(dgRequest);
                dgRequest.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTLOT"]);

                Util.gridClear(dgGrator);
                dgGrator.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTPROG"]);

                Util.gridClear(dgNotice);
                dgNotice.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTREF"]);

                txtNote.Text = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NOTE"].ToString();

                //txtCOST_CNTR_ID.Text = dsRslt.Tables["OUTDATA"].Rows[0]["COST_CNTR_ID"].ToString();
                //cboResnCode.SelectedValue = dsRslt.Tables["OUTDATA"].Rows[0]["RESNCODE"].ToString();

                //cboResnCode.IsEditable = false;
                //cboResnCode.IsHitTestVisible = false;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void Request()
        {
            string to = "";
            string carBonCopy = "";

            if (dgGrator.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1692");
                return;
            }
            if (dgRequest.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1748");
                return;
            }


            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("COST_CNTR_ID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));

            DataRow row;
            row = inDataTable.NewRow();
            row["APPR_BIZ_CODE"] = _requestType;// Util.GetCondition(cboReqType);
            row["REQ_NOTE"] = Util.GetCondition(txtNote);
            row["USERID"] = LoginInfo.USERID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            //row["RESNCODE"] = Util.GetCondition(cboResnCode, "SFU1593"); //사유는필수입니다. >> 사유를 선택하세요.
            //row["COST_CNTR_ID"] = Util.NVC(txtCOST_CNTR_ID.Tag);
           
            // Null 허용 필드
            //if (row["COST_CNTR_ID"].Equals("")) return;

            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(decimal));
            inLot.Columns.Add("WIPQTY2", typeof(decimal));
            inLot.Columns.Add("PRODID", typeof(string));
            inLot.Columns.Add("PRODNAME", typeof(string));
            inLot.Columns.Add("MODELID", typeof(string));
            for (int i = 0; i < dgRequest.Rows.Count; i++)
            {
                row = inLot.NewRow();
                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LOTID"));
                row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "REQQTY"));
                row["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "REQQTY")) *
                                 Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LANE_QTY")) *
                                 Util.NVC_Decimal(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LANE_PTN_QTY"));
                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODID"));
                row["PRODNAME"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODNAME"));
                row["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "MODELID"));
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
                    to = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                }
            }

            //참조자
            DataTable inRef = inData.Tables.Add("INREF");
            inRef.Columns.Add("REF_USERID", typeof(string));

            foreach (DataGridRow itemRow in dgNotice.Rows)
            {
                row = inRef.NewRow();
                row["REF_USERID"] = Util.NVC(DataTableConverter.GetValue(itemRow.DataItem, "USERID"));
                inRef.Rows.Add(row);

                carBonCopy += Util.NVC(DataTableConverter.GetValue(itemRow.DataItem, "USERID")) + ";";
            }

            const string bizRuleName = "BR_PRD_REG_APPR_REQUEST";

            try
            {
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INLOT,INPROG,INREF", "OUTDATA", inData);
                if (dsRslt.Tables[0].Rows.Count > 0)
                {
                    //GetLotList();
                    Util.gridClear(dgRequest);
                    Util.gridClear(dgGrator);
                    Util.gridClear(dgNotice);
                    MailSend mail = new MailSend();
                    string message = ObjectDic.Instance.GetObjectName("승인요청");
                    string title = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"] + " " + Header;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, to, carBonCopy, message, mail.makeBodyApp(title, Util.GetCondition(txtNote), inLot));
                }

                Util.MessageValidation("SFU1747");
                DialogResult = MessageBoxResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void ReqCancel()
        {

            const string bizRuleName = "DA_PRD_UPD_TB_SFC_APPR_REQ";

            string to = "";
            string carBonCopy = "";

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("REQ_NO", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["REQ_NO"] = _requestNo;
            dr["USERID"] = LoginInfo.USERID;
            dr["REQ_RSLT_CODE"] = "DEL";


            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

            for (int i = 0; i < dgGrator.Rows.Count; i++)
            {
                if (i == 0)//최초 승인자만 메일 가도록
                {
                    to = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                }
            }

            //참조자

            foreach (DataGridRow itemRow in dgNotice.Rows)
            {
                carBonCopy += Util.NVC(DataTableConverter.GetValue(itemRow.DataItem, "USERID")) + ";";
            }


            MailSend mail = new MailSend();
            string message = ObjectDic.Instance.GetObjectName("요청취소"); //승인요청취소
            string title = _requestNo + " " + Header;

            mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, to, carBonCopy, message, mail.makeBodyApp(title, Util.GetCondition(txtNote)));

            Util.MessageValidation("SFU1937");
            DialogResult = MessageBoxResult.OK;
            Close();
        }
        private void dgRequest_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            if (dg != null)
            {
                decimal dWipQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPQTY"));
                decimal dReqQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "REQQTY"));

                if (dReqQty <= 0 || dReqQty > dWipQty)
                {
                    Util.MessageValidation("SFU1749");
                    DataTableConverter.SetValue(dg.CurrentRow.DataItem, "REQQTY", dWipQty);

                    dg.CurrentRow.Refresh();
                    return;

                }

                decimal dLaneQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_QTY"));
                decimal dLanePtnQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_PTN_QTY"));
                DataTableConverter.SetValue(dg.CurrentRow.DataItem, "WIPQTY2", dReqQty * dLaneQty * dLanePtnQty);

                dg.CurrentRow.Refresh();
            }
        }
        private void dgRequest_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            C1.WPF.DataGrid.DataGridNumericColumn dc = dg.Columns["REQQTY"] as C1.WPF.DataGrid.DataGridNumericColumn;

            decimal dWipQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPQTY"));
            dc.Maximum = Convert.ToDouble(dWipQty);
            dc.Minimum = 0;
        }

        /*
        private void btnCodst_Click(object sender, RoutedEventArgs e)
        {
            CMM_COST_CNTR wndPopup = new CMM_COST_CNTR {FrameOperation = FrameOperation };
            wndPopup.FrameOperation = FrameOperation;

            object[] parameters = new object[1];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            C1WindowExtension.SetParameters(wndPopup, parameters);

            wndPopup.Closed += wndCodst_Closed;
            Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));

        }
        private void wndCodst_Closed(object sender, EventArgs e)
        {
            CMM_COST_CNTR window = sender as CMM_COST_CNTR;

            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {
                txtCOST_CNTR_ID.Tag = window.COST_CNTR_ID;
                txtCOST_CNTR_ID.Text = window.COST_CNTR_NAME;
            }
        }
        */

        //조용수 추가
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        private readonly CheckBox _chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        private void dgListHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try { 
                        if (string.IsNullOrEmpty(e.Column.Name) == false)
                        {
                            if (e.Column.Name.Equals("CHK"))
                            {
                                pre.Content = _chkAll;
                                e.Column.HeaderPresenter.Content = pre;
                                _chkAll.Checked -= checkAll_Checked;
                                _chkAll.Unchecked -= checkAll_Unchecked;
                                _chkAll.Checked += checkAll_Checked;
                                _chkAll.Unchecked += checkAll_Unchecked;
                            }
                        }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            bool bIsOcap = false;
            string sOcapLot = string.Empty;

            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

            foreach (DataRow row in dt.Rows)
            {
                if (Util.NVC(row["REL_SYSTEM_ID"]).Equals("OCAP"))
                {
                    bIsOcap = true;

                    if (string.IsNullOrEmpty(sOcapLot))
                        sOcapLot = Util.NVC(row["LOTID"]);
                    else
                        sOcapLot += "," + Util.NVC(row["LOTID"]);

                    row["CHK"] = false;
                }
                else
                {
                    row["CHK"] = true;
                }
            }

            //dt.Select("CHK = 0").ToList().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

            ChkAllSelect();

            if (bIsOcap)
                Util.MessageValidation("SUF9013", sOcapLot);   //OCAP 처리 대상은 요청 목록에서 제외됩니다. (LOT ID : %1)
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

            dt.Select("CHK = 1").ToList().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

            ChkAllClear();
        }
        private void ChkAllSelect()
        {
            Util.gridClear(dgRequest);

            DataTable dt = new DataTable();

            DataTable dtTo = DataTableConverter.Convert(dgListHold.ItemsSource);

            dtTo.Select("CHK = 0").ToList<DataRow>().ForEach(row => row.Delete());

            dt = dtTo.Copy();

            dgRequest.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void ChkAllClear()
        {
            Util.gridClear(dgRequest);
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

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }


        #endregion

        private void txtCSTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
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

                        if (sPasteStrings.Count() == 1)
                        {
                            GetLotList(sPasteStrings[0]);
                        }
                        else
                        {
                            foreach (string item in sPasteStrings)
                            {
                                sPasteStringCst += item + ",";
                            }

                            GetMultiLotList(string.Empty, sPasteStringCst);
                        }
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtCSTID.Text = string.Empty;
            txtLot.Text = string.Empty;
            txtModl.Text = string.Empty;
            txtPjt.Text = string.Empty;
            txtProd.Text = string.Empty;
            txtGrator.Text = string.Empty;
            txtNotice.Text = string.Empty;
            txtNote.Text = string.Empty;

            Util.gridClear(dgListHold);
            Util.gridClear(dgRequest);
            Util.gridClear(dgGrator);
            Util.gridClear(dgNotice);
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
    }
}
