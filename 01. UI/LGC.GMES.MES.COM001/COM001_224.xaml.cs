/*************************************************************************************
 Created Date : 2018.03.19
      Creator : 장만철
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
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
    public partial class COM001_224 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public COM001_224()
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
            //string[] sFilter = { "APPR_BIZ_CODE" };
            //_combo.SetCombo(cboReqTypeHist, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

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
            //listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기


            
        }

        #region [작업대상 목록 에서 선택]
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
                
                COM001_223_READ wndPopup = new COM001_223_READ();
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
            //승인하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU2878"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Accept();
                            }
                        });
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

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_APPR_BOX", "INDATA", "OUTDATA,BOX_INFO", dsRqst);

                Util.AlertInfo("SFU1690");  //승인되었습니다.

                MailSend mail = new CMM001.Class.MailSend();
                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                { //다음차수 안내메일
                    string sMsg =  ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, dsRslt.Tables["OUTDATA"].Rows[0]["APPR_USERID"].ToString(), GetCC(drChk[0]["REQ_NO"].ToString()), sMsg , mail.makeBodyApp(sTitle, Util.GetCondition(txtRemark), dsRslt.Tables["BOX_INFO"]));
                }
                else
                {  //완료메일
                    string sMsg = ObjectDic.Instance.GetObjectName("완료"); //승인완료
                    string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;
                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, drChk[0]["REQ_USER_ID"].ToString(), GetCC(drChk[0]["REQ_NO"].ToString()), sMsg, makeBodyApp(sTitle, Util.GetCondition(txtRemark), dsRslt.Tables["BOX_INFO"]));
                }

                //Util.AlertInfo("WIP 관리비즈룰 필요");
                GetList();
                Util.gridClear(dgAccept);
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public string makeBodyApp(string sTitle, string sContent, DataTable dtLot = null)
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
                    sb.Append("                    	<th>Lot ID</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("수량") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("제품ID") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("프로젝트명") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("창고명") + "</th>");
                    sb.Append("                    </tr>");
                    sb.Append("                </thead>");
                    sb.Append("                <tbody>");
                    foreach (DataRow dr in dtLot.Rows)
                    {
                        sb.Append("                	<tr>");
                        sb.Append("                     <td>" + Util.NVC(dr["BOXID"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC_NUMBER(dr["TOTAL_QTY2"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["PRODID"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["PRJT_NAME"]) + "&nbsp;</td>");
                        sb.Append("                     <td>" + Util.NVC(dr["WH_NAME"]) + "&nbsp;</td>");
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
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.ToString());
            }
            return sb.ToString();
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

                DataRow dr = dtRqst.NewRow();
                dr["REQ_NO"] = drChk[0]["REQ_NO"].ToString();
                dr["APPR_USERID"] = LoginInfo.USERID;
                dr["APPR_RSLT_CODE"] = "REJ";
                dr["APPR_NOTE"] = txtRemark.Text;

                dtRqst.Rows.Add(dr);

                dsRqst.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_APPR_BOX", "INDATA", "OUTDATA,BOX_INFO", dsRqst);

                MailSend mail = new CMM001.Class.MailSend();
                string sMsg = ObjectDic.Instance.GetObjectName("반려"); 
                string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;

                mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, drChk[0]["REQ_USER_ID"].ToString(), GetCC(drChk[0]["REQ_NO"].ToString()), sMsg, makeBodyApp(sTitle, Util.GetCondition(txtRemark), dsRslt.Tables["BOX_INFO"]));

                Util.AlertInfo("SFU1541");  //반려되었습니다.

                //Util.AlertInfo("WIP 관리비즈룰 필요");

                GetList();
                Util.gridClear(dgAccept);
                txtRemark.Text = "";
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region [작업대상 가져오기]
        public void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));               ;                
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;           
                dr["USERNAME"] = Util.GetCondition(txtReqUser);
                dr["USERID"] = LoginInfo.USERID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_TARGET_LIST", "INDATA", "OUTDATA", dtRqst);
             
                Util.GridSetData(dgList, dtRslt, FrameOperation);
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

                if (Util.GetCondition(txtBoxIDHist).Equals("")) //lot id 가 없는 경우
                {

                    dr["AREAID"] = Util.GetCondition(cboAreaHist, "SFU3203");//동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    dr["USERNAME"] = Util.GetCondition(txtReqUserHist);
                    dr["APPR_BIZ_CODE"] = "BOX_REQ_HOT";//Util.GetCondition(cboReqTypeHist, bAllNull: true);
                    dr["REQ_RSLT_CODE"] = Util.GetCondition(cboReqRsltHist, bAllNull: true);
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;                
                    dr["CSTID"] =null;

                    dtRqst.Rows.Add(dr);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtBoxIDHist);

                    dtRqst.Rows.Add(dr);
                }

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_APPR_BOX_HIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgListHist, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
