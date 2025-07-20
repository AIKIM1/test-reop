/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.03.15  LEEHJ     : 소형활성화 MES 복사
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_311_REQUEST1 : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;

        public FCS002_311_REQUEST1()
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

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            //CommonCombo _combo = new CommonCombo();


            ////동
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            ////라인
            //C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            ////공정
            //C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessChild = { cboEquipment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);

            ////설비
            //C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //요청구분
            //string[] sFilter = { "APPR_BIZ_CODE" };
            //_combo.SetCombo(cboReqType, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter:sFilter);

        }
        #endregion

        #region Event
        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    //사용자 권한별로 버튼 숨기기
        //    List<Button> listAuth = new List<Button>();
        //    //listAuth.Add(btnSave);
        //    Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        //    //여기까지 사용자 권한별로 버튼 숨기기
        //}

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            CommonCombo _combo = new CommonCombo();
            if (tmps != null && tmps.Length >= 1)
            {
                _reqNo = Util.NVC(tmps[0]);
                _reqType = Util.NVC(tmps[1]);
                
                //_combo.SetCombo(cboCostCenter, CommonCombo.ComboStatus.SELECT, sCase: "COST_CENTER");

                string[] sFilter1 = { "CHARGE_PROD_LOT" };
                _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter1);


            }
            if (!_reqNo.Equals("NEW"))
            {
                //_combo.SetCombo(cboCostCenter, CommonCombo.ComboStatus.SELECT, sCase: "COST_CENTER");

                string[] sFilter1 = { "CHARGE_PROD_LOT" };
                _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter1);

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

        #region [승인자 입력]
        private void txtGrator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
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
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                    GetLotList();
                    chkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region [대상 선택하기]
        //checked, unchecked 이벤트 사용하면 화면에서 사라지고 나타날때도
        //이벤트가 발생함 click 이벤트 잡아서 사용할것
        //WPF 그지같애
        private void chkHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;


                if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
                {
                    DataTable dtTo = DataTableConverter.Convert(dgRequest.ItemsSource);

                    if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                    {
                        dtTo.Columns.Add("CHK", typeof(Boolean));
                        dtTo.Columns.Add("EQSGNAME", typeof(string));
                        //dtTo.Columns.Add("EQPTNAME", typeof(string));
                        dtTo.Columns.Add("LOTID", typeof(string));
                        dtTo.Columns.Add("PRODID", typeof(string));
                        dtTo.Columns.Add("PRODNAME", typeof(string));
                        dtTo.Columns.Add("MODELID", typeof(string));
                        dtTo.Columns.Add("WIPQTY", typeof(decimal));
                        dtTo.Columns.Add("REQQTY", typeof(decimal));
                        dtTo.Columns.Add("WIPQTY2", typeof(decimal));
                        dtTo.Columns.Add("LANE_QTY", typeof(decimal));
                        dtTo.Columns.Add("LANE_PTN_QTY", typeof(decimal));
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [요청클릭]
        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
            }
        }


        #endregion

        #endregion

        #region Mehod

        #region [작업대상 가져오기]
        public void GetLotList(string lotLists = "", string CstList = "")
        {
            try
            {
                if (string.IsNullOrEmpty(lotLists) && string.IsNullOrEmpty(CstList) && Util.GetCondition(txtLot).Equals("") && Util.GetCondition(txtCSTID).Equals(""))
                {
                    //Util.MessageValidation("SFU4917"); //LOTID 또는 SKIDID를 입력하세요
                    return;
                } 
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("WIPHOLD", typeof(string));
                dtRqst.Columns.Add("SKIDID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = "WAIT"; //-WAIT : 공정 대기
                dr["WIPHOLD"] = "N"; //-현재 HOLD 아닌것들

                //if (Util.GetCondition(txtLot).Equals(""))
                //{
                //    if (!Util.GetCondition(txtCSTID).Equals(""))
                //        dr["SKIDID"] = Util.GetCondition(txtCSTID);
                //}
                //else
                //    dr["LOTID"] = Util.GetCondition(txtLot);

                if (!string.IsNullOrEmpty(lotLists))
                    dr["LOTID"] = lotLists;
                if (!string.IsNullOrEmpty(CstList))
                    dr["SKIDID"] = CstList;
                if (!Util.GetCondition(txtLot).Equals(""))
                    dr["LOTID"] = Util.GetCondition(txtLot);
                if (!Util.GetCondition(txtCSTID).Equals(""))
                    dr["SKIDID"] = Util.GetCondition(txtCSTID);


                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST", "INDATA", "OUTDATA", dtRqst);

                Util.gridClear(dgListHold);
                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU2024");  //해당 LOT은 현재 WAIT 상태가 아니거나 HOLD 상태 입니다.
                }
                else
                {
                    dgListHold.ItemsSource = DataTableConverter.Convert(dtRslt);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

                txtCOST_CNTR_ID.Text = dsRslt.Tables["OUTDATA"].Rows[0]["COST_CNTR_ID"].ToString();
                cboResnCode.SelectedValue = dsRslt.Tables["OUTDATA"].Rows[0]["RESNCODE"].ToString();

                cboResnCode.IsEditable = false;
                cboResnCode.IsHitTestVisible = false;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        private void Request()
        {
            string sTo = "";
            string sCC = "";

            if (dgGrator.Rows.Count == 0)
            {
                Util.Alert("SFU1692");  //승인자가 필요합니다.
                return;
            }
            if (dgRequest.Rows.Count == 0)
            {
                Util.Alert("SFU1748");  //요청 목록이 필요합니다.
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

            DataRow row = null;
            row = inDataTable.NewRow();
            row["APPR_BIZ_CODE"] = _reqType;// Util.GetCondition(cboReqType);
            row["REQ_NOTE"] = Util.GetCondition(txtNote);
            row["USERID"] = LoginInfo.USERID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["RESNCODE"] = Util.GetCondition(cboResnCode, "SFU1593"); //사유는필수입니다. >> 사유를 선택하세요.

            row["COST_CNTR_ID"] = Util.NVC(txtCOST_CNTR_ID.Tag);
           
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
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_APPR_REQUEST", "INDATA,INLOT,INPROG,INREF", "OUTDATA", inData);
                if (dsRslt.Tables[0].Rows.Count > 0)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(dsRslt.Tables[0].Rows[0]["REQ_NO"].ToString(), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                    GetLotList();
                    Util.gridClear(dgRequest);
                    Util.gridClear(dgGrator);
                    Util.gridClear(dgNotice);
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString() + " " + this.Header;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote), inLot));
                }
                Util.AlertInfo("SFU1747");  //요청되었습니다.
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
                //Util.AlertInfo("WIP 관리비즈룰 필요");
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //Util.Alert(ex.ToString());
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

            mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote)));

            Util.AlertInfo("SFU1937");  //취소되었습니다.
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void dgRequest_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            Decimal dWipQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPQTY"));
            Decimal dReqQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "REQQTY"));

            if (dReqQty <= 0 || dReqQty > dWipQty)
            {
                Util.AlertInfo("SFU1749");  //요청 수량이 잘못되었습니다.

                DataTableConverter.SetValue(dg.CurrentRow.DataItem, "REQQTY", dWipQty);

                dg.CurrentRow.Refresh();
                return;

            }

            Decimal dLaneQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_QTY"));
            Decimal dLanePtnQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_PTN_QTY"));
            DataTableConverter.SetValue(dg.CurrentRow.DataItem, "WIPQTY2", dReqQty * dLaneQty * dLanePtnQty);
            dg.CurrentRow.Refresh();
        }

        #endregion

        private void dgRequest_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            C1.WPF.DataGrid.DataGridNumericColumn dc = dg.Columns["REQQTY"] as C1.WPF.DataGrid.DataGridNumericColumn;

            Decimal dWipQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPQTY"));
            dc.Maximum = Convert.ToDouble(dWipQty);
            dc.Minimum = 0;
        }

        private void btnCodst_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_COST_CNTR wndPopup = new CMM001.Popup.CMM_COST_CNTR();
            wndPopup.FrameOperation = this.FrameOperation;
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndCodst_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
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
                }catch(Exception ex)
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

        private void txtLot_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    string lotList = string.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (i == 0)
                        {
                            lotList = sPasteStrings[i];
                        }
                        else
                        {
                            lotList = lotList + "," + sPasteStrings[i];
                        }

                        System.Windows.Forms.Application.DoEvents();

                    }

                    GetLotList(lotList,string.Empty);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtCSTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    string CstList = string.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (i == 0)
                        {
                            CstList = sPasteStrings[i];
                        }
                        else
                        {
                            CstList = CstList + "," + sPasteStrings[i];
                        }

                        System.Windows.Forms.Application.DoEvents();

                     
                    }
                    GetLotList(string.Empty,CstList);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtCSTID.Text = string.Empty;
            txtLot.Text = string.Empty;
            txtGrator.Text = string.Empty;
            txtNotice.Text = string.Empty;
            txtNote.Text = string.Empty;
            txtCOST_CNTR_ID.Text = string.Empty;
            txtCOST_CNTR_ID.Tag = null;
            cboResnCode.SelectedIndex = 0;
            chkAll.IsChecked = false;
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
                    Util.MessageValidation("SUF4969");
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
