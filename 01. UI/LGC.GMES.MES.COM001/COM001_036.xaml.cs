/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.08.26  안유수S     C20220826-000411   LOT 릴리즈/폐기/물품청구 승인 화면에서 WRITE 권한 없이 승인, 반려 처리할 수 있는 오류 수정
  2023.07.27  문혜림      E20230718-000593   전공정 LOSS 처리 시 공정 진행중 LOT에 대한 승인 방어 로직 추가



 
**************************************************************************************/

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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_036 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string sREQ_NO = string.Empty;

        public COM001_036()
        {
            InitializeComponent();

            //ldpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7) ;
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

            _combo.SetCombo(cboAreaHist, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            //요청구분
            string[] sFilter = { "APPR_BIZ_CODE" };
            _combo.SetCombo(cboReqTypeHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            //요청구분
            string[] sFilter1 = { "REQ_RSLT_CODE" };
            _combo.SetCombo(cboReqRsltHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnAccept);
            listAuth.Add(btnReject);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기



        }

        #region [작업대상 목록 에서 선택]
        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0) ||
               DataTableConverter.GetValue(rb.DataContext, "CHK").ToString().Equals("False")) // 2024.10.21 김영국 - CHK값이 true, false로 들어오는 문제점 대응로직.
            {
                //체크시 처리될 로직
                string sReqNo = DataTableConverter.GetValue(rb.DataContext, "REQ_NO").ToString();

                //승인내용 조회
                GetApprovalList(sReqNo);

                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                txtRemark.Text = "";

                #region [Lot Release 시 선입선출제외요청 호출(C20180430_75973)]
                string ValueToBIZ_CODE = DataTableConverter.GetValue(rb.DataContext, "APPR_BIZ_CODE").ToString();
                sREQ_NO = sReqNo;
                if (string.Equals(ValueToBIZ_CODE, "LOT_RELEASE"))
                {
                    chkFIFO.IsEnabled = true;
                }                    
                else
                {
                    chkFIFO.IsEnabled = false;
                    chkFIFO.IsChecked = false;
                }                    
                #endregion
            }
        }
        #endregion

        #region [조회클릭]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        private void txtReqUser_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetList();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region [리스트 더블클릭]
        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList.CurrentRow != null && dgList.CurrentColumn.Name.Equals("REQ_NO"))
            {
                
                COM001_035_READ wndPopup = new COM001_035_READ();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_NO"));

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
                
            }
        }

        #endregion

        #region [링크글씨 색 바꾸기]
        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("REQ_NO"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));
        }

        #endregion

        #region [승인 클릭]
        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgList, "CHK");
                if (drChk.Length == 0)
                {
                    Util.AlertInfo("SFU1654");  //선택된 요청이 없습니다.
                    return;
                }

                string sMsg = "SFU2878"; //승인하시겠습니까?
                string sLot = string.Empty;
                string sWipstat = string.Empty;

                // 전공정 Loss 승인 처리 시 재공 종료 체크.
                if (Util.NVC(drChk[0]["APPR_BIZ_CODE"]).Equals("LOT_SCRAP_YIELD"))
                {
                    if (ChkTermWip(Util.NVC(drChk[0]["REQ_NO"]), out sLot))
                        sMsg = "SFU5150"; // 종료된 LOT[%1]이 존재 합니다. 승인 진행하시겠습니까?

                    // 대기, 완성 재공 아닌 경우 승인 거부
                    else if (ChkWaitEndWip(Util.NVC(drChk[0]["REQ_NO"]), out sLot, out sWipstat))
                    {
                        Util.MessageValidation("SFU9120", sLot, sWipstat); // LOT [%1]의 WIP 상태가 [%2] 입니다. 반려해주세요.
                        return;
                    }
                }

                Util.MessageConfirm(sMsg, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Accept();
                        #region [Lot Release 시 선입선출제외요청 호출(C20180430_75973)]
                        if ((bool)chkFIFO.IsChecked)
                            callRequestHot();
                        #endregion
                    }
                }, sLot,sWipstat);

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region [반려클릭]
        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            //반려하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU2866"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Reject();
                            }
                        });
        }


        #endregion
        #endregion

        #region Mehod
        private void Accept()
        {
            DataRow[] drChk = Util.gridGetChecked(ref dgList, "CHK");

            try
            {
                if (drChk.Length == 0)
                {
                    Util.AlertInfo("SFU1654");  //선택된 요청이 없습니다.
                    return;
                }

                DataSet dsRqst = new DataSet();

                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "INDATA";

                dtRqst.Columns.Add("REQ_NO", typeof(string));
                dtRqst.Columns.Add("APPR_USERID", typeof(string));
                dtRqst.Columns.Add("APPR_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("APPR_NOTE", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));
                //dtRqst.Columns.Add("RESNCODE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["REQ_NO"] = drChk[0]["REQ_NO"].ToString();
                dr["APPR_USERID"] = LoginInfo.USERID;
                dr["APPR_RSLT_CODE"] = "APP";
                dr["APPR_NOTE"] = Util.GetCondition(txtRemark);
                dr["APPR_BIZ_CODE"] = drChk[0]["APPR_BIZ_CODE"].ToString();
                //dr["RESNCODE"] = drChk[0]["RESNCODE"].ToString();
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                dsRqst.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_APPR", "INDATA", "OUTDATA,LOT_INFO", dsRqst);

                Util.AlertInfo("SFU1690");  //승인되었습니다.

                MailSend mail = new CMM001.Class.MailSend();
                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                { //다음차수 안내메일
                    string sMsg =  ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, dsRslt.Tables["OUTDATA"].Rows[0]["APPR_USERID"].ToString(), GetCC(drChk[0]["REQ_NO"].ToString()), sMsg , mail.makeBodyApp(sTitle, Util.GetCondition(txtRemark), dsRslt.Tables["LOT_INFO"]));
                }
                else
                {  //완료메일
                    string sMsg = ObjectDic.Instance.GetObjectName("완료"); //승인완료
                    string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;
                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, drChk[0]["REQ_USER_ID"].ToString(), GetCC(drChk[0]["REQ_NO"].ToString()), sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtRemark), dsRslt.Tables["LOT_INFO"]));
                }

                //Util.AlertInfo("WIP 관리비즈룰 필요");
                GetList();
                Util.gridClear(dgAccept);
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Reject()
        {
            DataRow[] drChk = Util.gridGetChecked(ref dgList, "CHK");

            try
            {
                if (drChk.Length == 0)
                {
                    Util.AlertInfo("SFU1654");  //선택된 요청이 없습니다.
                    return;
                }

                DataSet dsRqst = new DataSet();

                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "INDATA";

                dtRqst.Columns.Add("REQ_NO", typeof(string));
                dtRqst.Columns.Add("APPR_USERID", typeof(string));
                dtRqst.Columns.Add("APPR_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("APPR_NOTE", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["REQ_NO"] = drChk[0]["REQ_NO"].ToString();
                dr["APPR_USERID"] = LoginInfo.USERID;
                dr["APPR_RSLT_CODE"] = "REJ";
                dr["APPR_NOTE"] = txtRemark.Text;
                dr["APPR_BIZ_CODE"] = drChk[0]["APPR_BIZ_CODE"].ToString();

                dtRqst.Rows.Add(dr);

                dsRqst.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_APPR", "INDATA", "OUTDATA,LOT_INFO", dsRqst);

                MailSend mail = new CMM001.Class.MailSend();
                string sMsg = ObjectDic.Instance.GetObjectName("반려"); 
                string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;

                mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, drChk[0]["REQ_USER_ID"].ToString(), GetCC(drChk[0]["REQ_NO"].ToString()), sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtRemark), dsRslt.Tables["LOT_INFO"]));

                Util.AlertInfo("SFU1541");  //반려되었습니다.

                //Util.AlertInfo("WIP 관리비즈룰 필요");

                GetList();
                Util.gridClear(dgAccept);
                txtRemark.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [작업대상 가져오기]
        public void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                //dtRqst.Columns.Add("FROM_DATE", typeof(string));
                //dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["FROM_DATE"] = Util.GetCondition(ldpDateFrom);
                //dr["TO_DATE"] = Util.GetCondition(ldpDateTo);
                dr["USERNAME"] = Util.GetCondition(txtReqUser);
                dr["USERID"] = LoginInfo.USERID;
                //dr["USERID"] = "s.forusystem";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_TARGET_LIST", "INDATA", "OUTDATA", dtRqst);

                //DataColumn dcChk = new DataColumn("CHK", typeof(Boolean));
                //dcChk.DefaultValue = false;

                //dtRslt.Columns.Add(dcChk);

                //Util.gridClear(dgList);
                //dgList.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgList, dtRslt, FrameOperation);

                if (dgList.Rows.Count > 0) DataTableConverter.SetValue(dgList.Rows[0].DataItem, "CHK", false);

                Util.gridClear(dgAccept);

                txtRemark.Text = "";
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #region [승인내용 가져오기]
        private void GetApprovalList(string sReqNo)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = sReqNo;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_DETAIL_LIST", "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgAccept);
                //dgAccept.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgAccept, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [참조자 가져오기]
        private string GetCC(string sReqNo)
        {
            string sCC = "";
            try
            {
                

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = sReqNo;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_APPR_REF", "INDATA", "OUTDATA", dtRqst);

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    sCC += dtRslt.Rows[i]["USERID"].ToString() + ";";
                }

                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            
            return sCC;
            
        }


        #endregion

        private void callRequestHot()
        {
            COM001_035_REQUEST_HOT wndPopup = new COM001_035_REQUEST_HOT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = sREQ_NO;
                Parameters[1] = "LOT_REQ_HOT";
                Parameters[2] = "LOT_RELEASE_REQ_HOT";  // LOT Release 후 선입선출제외요청 구분용
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupHot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }
        private void wndPopupHot_Closed(object sender, EventArgs e)
        {
            COM001_035_REQUEST_HOT window = sender as COM001_035_REQUEST_HOT;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                #region [Lot Release 시 선입선출제외요청 호출(C20180430_75973)]
                chkFIFO.IsChecked = false;
                chkFIFO.IsEnabled = false;
                #endregion
            }
        }


        private bool ChkTermWip(string sReqNo, out string sLot)
        {
            sLot = "";
            try
            {
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("REQ_NO", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["REQ_NO"] = sReqNo;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_TERM_LOT", "INDATA", "OUTDATA", inTable);

                foreach (DataRow dr in dtRslt.Rows)
                {
                    bRet = true;

                    if (sLot.Equals(""))
                        sLot = Util.NVC(dr["LOTID"]);
                    else
                        sLot = sLot + ", " + Util.NVC(dr["LOTID"]);
                }
                
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool ChkWaitEndWip(string sReqNo, out string sLot, out string sWipstat)
        {
            sLot = "";
            sWipstat = "";
            try
            {
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("REQ_NO", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["REQ_NO"] = sReqNo;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_APPR_REQ_LOT_INFO", "RQSTDT", "RSLTDT", inTable);

                foreach (DataRow dr in dtRslt.Rows)
                {
                    if (!(dr["WIPSTAT"].ToString().Equals("WAIT") || dr["WIPSTAT"].ToString().Equals("END")))
                    {
                        bRet = true;
                        sLot = Util.NVC(dr["LOTID"]);
                        sWipstat = Util.NVC(dr["WIPSTAT"]);
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            GetListHist();
        }

        public void GetListHist()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                if (Util.GetCondition(txtLotIDHist).Equals("")) //lot id 가 없는 경우
                {

                    dr["AREAID"] = Util.GetCondition(cboAreaHist, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    dr["USERNAME"] = Util.GetCondition(txtReqUserHist);
                    dr["APPR_BIZ_CODE"] = Util.GetCondition(cboReqTypeHist, bAllNull: true);
                    dr["REQ_RSLT_CODE"] = Util.GetCondition(cboReqRsltHist, bAllNull: true);
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;

                    if (!Util.GetCondition(txtCSTIDHist).Equals(""))
                        dr["CSTID"] = Util.GetCondition(txtCSTIDHist);

                    dtRqst.Rows.Add(dr);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLotIDHist);

                    dtRqst.Rows.Add(dr);
                }

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_APPR_HIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgListHist, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
