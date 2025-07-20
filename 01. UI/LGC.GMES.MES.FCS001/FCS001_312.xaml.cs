/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : BizWF & 전공정 Loss 승인
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.08.26  안유수S     C20220826-000411   LOT 릴리즈/폐기/물품청구 승인 화면에서 WRITE 권한 없이 승인, 반려 처리할 수 있는 오류 수정
  2023.03.16  이홍주    : 소형활성화MES 복사
  2023.07.17  조영대    : FCS002_312 => FCS001_312 복사
  2024.09.09  조영대    : 이력 Tab의 Lot ID => Tray Lot ID 로 수정
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_312 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string sREQ_NO = string.Empty;
        private int countProcessPerOnce = 20;

        public FCS001_312()
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

            _combo.SetCombo(cboAreaHist, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            //요청구분
            cboReqTypeHist.SetCommonCode("APPR_BIZ_CODE", "ATTR2='Y'", CommonCombo.ComboStatus.ALL, false);

            //상태
            string[] sFilter1 = { "REQ_RSLT_CODE" };
            _combo.SetCombo(cboReqRsltHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnAccept);
            listAuth.Add(btnReject);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            dtpSearchDate.SelectedToDateTime = DateTime.Today;
            dtpSearchDate.SelectedFromDateTime = DateTime.Today.AddDays(-7);
        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

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

                // 전공정 Loss 승인 처리 시 재공 종료 체크.
                if (Util.NVC(drChk[0]["APPR_BIZ_CODE"]).Equals("LOT_SCRAP_YIELD"))
                    if (ChkTermWip(Util.NVC(drChk[0]["REQ_NO"]), out sLot))
                        sMsg = "SFU5150"; // 종료된 LOT[%1]이 존재 합니다. 승인 진행하시겠습니까? 


                Util.MessageConfirm(sMsg, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Accept();
                    }
                }, sLot);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            GetListHist();
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList.CurrentRow != null && dgList.CurrentColumn.Name.Equals("REQ_NO"))
            {

                FCS001_311_READ wndPopup = new FCS001_311_READ();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "REQ_NO"));

                    Parameters[0] = dgList.GetValue("REQ_NO").Nvc();
                    Parameters[1] = dgList.GetValue("REQ_RSLT_CODE").Nvc();
                    Parameters[2] = dgList.GetValue("APPR_BIZ_CODE").Nvc();
                    Parameters[3] = dgList.GetValue("APPR_NAME").Nvc();

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }

            }
        }

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

        private void dgList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            btnSearch.IsEnabled = true;
        }

        private void dgListHist_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            btnSearchHist.IsEnabled = true;
        }

        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
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
                    chkFIFO.Visibility = Visibility.Visible;
                    chkFIFO.IsEnabled = true;
                }                    
                else
                {
                    chkFIFO.Visibility = Visibility.Collapsed;
                    chkFIFO.IsEnabled = false;
                    chkFIFO.IsChecked = false;
                }                    
                #endregion
            }
        }

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
                Util.MessageException(ex);
            }
        }
        
        private object xProgress_WorkProcess(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
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

                            DataSet indataSet = new DataSet();
                            indataSet.Tables.Add(inDataTable);
                            indataSet.Tables.Add(dtInCell);

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
                                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(BizRuleID, "INDATA,INCELL", "OUTDATA", indataSet);
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

                            DataSet indataSet = new DataSet();
                            indataSet.Tables.Add(inDataTable);
                            indataSet.Tables.Add(dtInCell);

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
                                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(BizRuleID, "INDATA,INCELL", "OUTDATA", indataSet);
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

        private void xProgress_WorkProcessChanged(object sender, int percent, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
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

        private void xProgress_WorkProcessCompleted(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                string workType = arguments[0] as string;

                if (e.Result != null && e.Result is string)
                {
                    if (e.Result.Nvc().Equals("SUCCESS"))
                    {
                        Util.AlertInfo("SFU1541");  //반려되었습니다.

                        GetList();

                        Util.gridClear(dgAccept);
                        txtRemark.Text = "";
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
                btnAccept.IsEnabled = btnReject.IsEnabled = btnSearch.IsEnabled = true;
            }
        }
        
        #endregion

        #region Mehod

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERNAME"] = Util.GetCondition(txtReqUser);
                dr["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(dr);

                Util.gridClear(dgAccept);

                txtRemark.Text = string.Empty;

                btnSearch.IsEnabled = false;
                dgList.ExecuteService("DA_SEL_APPROVAL_TARGET_LIST", "INDATA", "OUTDATA", dtRqst);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

                dgAccept.ExecuteService("DA_PRD_SEL_APPROVAL_DETAIL_LIST", "INDATA", "OUTDATA", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetReferenceList(string sReqNo)
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
                Util.MessageException(ex);
            }

            return sCC;

        }

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
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["REQ_NO"] = drChk[0]["REQ_NO"].ToString();
                dr["APPR_USERID"] = LoginInfo.USERID;
                dr["APPR_RSLT_CODE"] = "APP";
                dr["APPR_NOTE"] = Util.GetCondition(txtRemark);
                dr["APPR_BIZ_CODE"] = drChk[0]["APPR_BIZ_CODE"].ToString();
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                dsRqst.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_FORM_APPR_REJECT", "INDATA", "OUTDATA,OUTLOT,OUTCELL", dsRqst);

                Util.AlertInfo("SFU1690");  //승인되었습니다.

                MailSend mail = new CMM001.Class.MailSend();
                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                { //다음차수 안내메일
                    string sMsg =  ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, dsRslt.Tables["OUTDATA"].Rows[0]["APPR_USERID"].ToString(), GetReferenceList(drChk[0]["REQ_NO"].ToString()), sMsg , mail.makeBodyApp(sTitle, Util.GetCondition(txtRemark), dsRslt.Tables["LOT_INFO"]));
                }
                else
                {  //완료메일
                    string sMsg = ObjectDic.Instance.GetObjectName("완료"); //승인완료
                    string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;
                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, drChk[0]["REQ_USER_ID"].ToString(), GetReferenceList(drChk[0]["REQ_NO"].ToString()), sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtRemark), dsRslt.Tables["LOT_INFO"]));
                }

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
                dr["APPR_BIZ_CODE"] = drChk[0]["APPR_BIZ_CODE"].Nvc();

                dtRqst.Rows.Add(dr);

                dsRqst.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_FORM_APPR_REJECT", "INDATA", "OUTDATA,OUTLOT,OUTCELL", dsRqst);
                if (dsRslt != null && dsRslt.Tables.Contains("OUTCELL") && dsRslt.Tables["OUTCELL"].Rows.Count > 0)
                {
                    #region 반려 메일 발송
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("반려");
                    string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, drChk[0]["REQ_USER_ID"].ToString(), GetReferenceList(drChk[0]["REQ_NO"].ToString()), sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtRemark), dsRslt.Tables["LOT_INFO"]));
                    #endregion

                    switch (drChk[0]["APPR_BIZ_CODE"].Nvc())
                    {
                        case "REQUEST_BIZWF_LOT":
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

                                foreach (DataRow drCell in dsRslt.Tables["OUTCELL"].Rows)
                                {
                                    DataRow RowCell = dtInCell.NewRow();
                                    RowCell["SUBLOTID"] = drCell["SUBLOTID"];
                                    dtInCell.Rows.Add(RowCell);
                                }

                                if (inDataSet.Tables["INCELL"].Rows.Count > 0)
                                {
                                    object[] argument = new object[3] { "복구", inDataSet.Tables["INDATA"].Copy(), inDataSet.Tables["INCELL"].Copy() };

                                    xProgress.Percent = 0;
                                    xProgress.ProgressText = MessageDic.Instance.GetMessage("10057") + " - 0 / " + inDataSet.Tables["INCELL"].Rows.Count.Nvc();
                                    xProgress.Visibility = Visibility.Visible;

                                    btnAccept.IsEnabled = btnReject.IsEnabled = btnSearch.IsEnabled = false;
                                    xProgress.RunWorker(argument);
                                }
                                #endregion
                            }
                            break;
                        case "REQUEST_CANCEL_BIZWF_LOT":
                            {
                                #region 개발/기술 Sample Cell 추출 처리
                                bool isGood = true;

                                DataSet indataSet = new DataSet();
                                DataTable dtIndata = indataSet.Tables.Add("INDATA");
                                dtIndata.Columns.Add("USERID", typeof(string));
                                dtIndata.Columns.Add("TD_FLAG", typeof(string));
                                dtIndata.Columns.Add("SPLT_FLAG", typeof(string));
                                dtIndata.Columns.Add("MENUID", typeof(string));
                                dtIndata.Columns.Add("USER_IP", typeof(string));
                                dtIndata.Columns.Add("PC_NAME", typeof(string));

                                DataRow InRow = dtIndata.NewRow();
                                InRow["USERID"] = LoginInfo.USERID;
                                InRow["TD_FLAG"] = dsRslt.Tables["OUTDATA"].Rows[0]["TD_FLAG"].Nvc();
                                InRow["SPLT_FLAG"] = dsRslt.Tables["OUTDATA"].Rows[0]["SPLT_FLAG"].Nvc();
                                InRow["MENUID"] = LoginInfo.CFG_MENUID;
                                InRow["USER_IP"] = LoginInfo.USER_IP;
                                InRow["PC_NAME"] = LoginInfo.PC_NAME;
                                dtIndata.Rows.Add(InRow);

                                DataTable dtProcessCell = new DataTable("INCELL");
                                dtProcessCell.Columns.Add("SUBLOTID", typeof(string));
                                dtProcessCell.Columns.Add("UNPACK_CELL_YN", typeof(string));

                                foreach (DataRow drCell in dsRslt.Tables["OUTCELL"].Rows)
                                {
                                    DataRow RowCell = dtProcessCell.NewRow();
                                    RowCell["SUBLOTID"] = drCell["SUBLOTID"];
                                    RowCell["UNPACK_CELL_YN"] = drCell["UNPACK_CELL_YN"];
                                    dtProcessCell.Rows.Add(RowCell);

                                    isGood = drCell["LOT_TYPE"].Equals("G");
                                }

                                if (dtProcessCell.Rows.Count > 0)
                                {
                                    object[] argument = new object[4] { "추출", dtIndata.Copy(), dtProcessCell.Copy(), isGood };

                                    xProgress.Percent = 0;
                                    xProgress.ProgressText = MessageDic.Instance.GetMessage("10057") + " - 0 / " + dtProcessCell.Rows.Count.Nvc();
                                    xProgress.Visibility = Visibility.Visible;

                                    xProgress.RunWorker(argument);
                                }

                                #endregion
                            }
                            break;
                    }
                }          
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void wndPopupHot_Closed(object sender, EventArgs e)
        {
            //FCS001_311_REQUEST_HOT window = sender as FCS001_311_REQUEST_HOT;

            //if (window.DialogResult == MessageBoxResult.OK)
            //{
            //    #region [Lot Release 시 선입선출제외요청 호출(C20180430_75973)]
            //    chkFIFO.IsChecked = false;
            //    chkFIFO.IsEnabled = false;
            //    #endregion
            //}
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

        private void GetListHist()
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
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                if (string.IsNullOrEmpty(txtCellIDHist.Text)) //lot id 가 없는 경우
                {

                    dr["AREAID"] = Util.GetCondition(cboAreaHist, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["FROM_DATE"] = dtpSearchDate.SelectedFromDateTime.ToString("yyyy-MM-dd");
                    dr["TO_DATE"] = dtpSearchDate.SelectedToDateTime.ToString("yyyy-MM-dd");
                    dr["USERNAME"] = txtReqUserHist.GetBindValue();
                    dr["APPR_BIZ_CODE"] = cboReqTypeHist.GetBindValue();
                    dr["REQ_RSLT_CODE"] = cboReqRsltHist.GetBindValue();
                    dr["PRODID"] = txtProdID.GetBindValue();

                    dtRqst.Rows.Add(dr);
                }
                else //Cell Id 가 있는경우 다른 조건 모두 무시
                {
                    dr["SUBLOTID"] = txtCellIDHist.GetBindValue();

                    dtRqst.Rows.Add(dr);
                }

                btnSearchHist.IsEnabled = false;
                dgListHist.ExecuteService("DA_SEL_APPROVAL_APPR_HIST", "INDATA", "OUTDATA", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    }
}
